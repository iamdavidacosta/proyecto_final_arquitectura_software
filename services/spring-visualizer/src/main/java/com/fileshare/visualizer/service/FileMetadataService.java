package com.fileshare.visualizer.service;

import com.fileshare.visualizer.dto.FileInfoDto;
import com.fileshare.visualizer.model.FileMetadata;
import com.fileshare.visualizer.repository.FileMetadataRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.stream.Collectors;

@Slf4j
@Service
@RequiredArgsConstructor
public class FileMetadataService {

    private final FileMetadataRepository repository;

    public List<FileInfoDto> getAllFiles() {
        log.info("Fetching all files from MongoDB");
        List<FileMetadata> files = repository.findAllByOrderByCreatedAtDesc();
        log.info("Found {} files in database", files.size());
        return files.stream()
                .map(this::toDto)
                .collect(Collectors.toList());
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
