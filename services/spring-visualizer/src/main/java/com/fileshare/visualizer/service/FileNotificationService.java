package com.fileshare.visualizer.service;

import com.fileshare.visualizer.dto.FileInfoDto;
import com.fileshare.visualizer.dto.FileUpdateEvent;
import com.fileshare.visualizer.model.FileMetadata;
import com.fileshare.visualizer.repository.FileMetadataRepository;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.scheduling.annotation.EnableScheduling;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.stream.Collectors;

/**
 * Servicio que detecta cambios en archivos y envía notificaciones WebSocket.
 * Usa polling periódico para detectar cambios en MongoDB.
 */
@Slf4j
@Service
@EnableScheduling
@RequiredArgsConstructor
public class FileNotificationService {

    private final SimpMessagingTemplate messagingTemplate;
    private final FileMetadataRepository repository;

    private int lastKnownCount = 0;
    private LocalDateTime lastCheck = LocalDateTime.now();

    @PostConstruct
    public void init() {
        lastKnownCount = (int) repository.count();
        log.info("FileNotificationService initialized. Current file count: {}", lastKnownCount);
    }

    /**
     * Verificar cambios cada 2 segundos y enviar actualizaciones si hay cambios
     */
    @Scheduled(fixedRate = 2000)
    public void checkForChanges() {
        try {
            int currentCount = (int) repository.count();
            
            if (currentCount != lastKnownCount) {
                log.info("File count changed from {} to {}", lastKnownCount, currentCount);
                
                String eventType = currentCount > lastKnownCount ? "INSERT" : "DELETE";
                
                // Enviar evento de cambio
                FileUpdateEvent event = FileUpdateEvent.builder()
                        .eventType(eventType)
                        .timestamp(LocalDateTime.now())
                        .build();
                
                messagingTemplate.convertAndSend("/topic/files", event);
                
                // Enviar lista actualizada
                sendFilesList();
                
                lastKnownCount = currentCount;
            }
        } catch (Exception e) {
            log.debug("Error checking for file changes: {}", e.getMessage());
        }
    }

    /**
     * Enviar la lista completa de archivos a todos los clientes conectados
     */
    public void sendFilesList() {
        try {
            List<FileInfoDto> files = repository.findAllByOrderByCreatedAtDesc()
                    .stream()
                    .map(this::toDto)
                    .collect(Collectors.toList());
            
            messagingTemplate.convertAndSend("/topic/files-list", files);
            log.debug("Sent files list via WebSocket ({} files)", files.size());
        } catch (Exception e) {
            log.error("Error sending files list: {}", e.getMessage());
        }
    }

    private FileInfoDto toDto(FileMetadata metadata) {
        return FileInfoDto.builder()
                .fileId(metadata.getFileId() != null ? metadata.getFileId().toString() : null)
                .userId(metadata.getUserId() != null ? metadata.getUserId().toString() : null)
                .fileName(metadata.getOriginalFileName())
                .contentType(metadata.getContentType())
                .fileSize(metadata.getFileSize())
                .hash(metadata.getHash())
                .isEncrypted(metadata.getIsEncrypted())
                .description(metadata.getDescription())
                .status(metadata.getStatus())
                .createdAt(metadata.getCreatedAt())
                .processedAt(metadata.getProcessedAt())
                .build();
    }
}
