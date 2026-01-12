package com.fileshare.visualizer.controller;

import com.fileshare.visualizer.dto.DownloadUrlDto;
import com.fileshare.visualizer.dto.FileInfoDto;
import com.fileshare.visualizer.service.SoapClientService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@Slf4j
@RestController
@RequestMapping("/api/visualizer")
@RequiredArgsConstructor
@CrossOrigin(origins = "*")
public class VisualizerController {

    private final SoapClientService soapClientService;

    @GetMapping("/files/{fileId}")
    public ResponseEntity<FileInfoDto> getFile(@PathVariable String fileId) {
        log.info("REST request to get file via SOAP: {}", fileId);
        
        FileInfoDto file = soapClientService.getFile(fileId);
        
        if (file == null) {
            return ResponseEntity.notFound().build();
        }
        
        return ResponseEntity.ok(file);
    }

    @GetMapping("/users/{userId}/files")
    public ResponseEntity<List<FileInfoDto>> getUserFiles(@PathVariable String userId) {
        log.info("REST request to get user files via SOAP: {}", userId);
        
        List<FileInfoDto> files = soapClientService.getUserFiles(userId);
        return ResponseEntity.ok(files);
    }

    @GetMapping("/files/{fileId}/download")
    public ResponseEntity<DownloadUrlDto> getDownloadUrl(
            @PathVariable String fileId,
            @RequestParam(defaultValue = "3600") int expiryInSeconds) {
        log.info("REST request to get download URL via SOAP: {}", fileId);
        
        DownloadUrlDto downloadUrl = soapClientService.getDownloadUrl(fileId, expiryInSeconds);
        
        if (downloadUrl == null) {
            return ResponseEntity.notFound().build();
        }
        
        return ResponseEntity.ok(downloadUrl);
    }

    @DeleteMapping("/files/{fileId}")
    public ResponseEntity<Map<String, Boolean>> deleteFile(
            @PathVariable String fileId,
            @RequestParam String userId) {
        log.info("REST request to delete file via SOAP: {} for user: {}", fileId, userId);
        
        boolean success = soapClientService.deleteFile(fileId, userId);
        return ResponseEntity.ok(Map.of("success", success));
    }

    @GetMapping("/health")
    public ResponseEntity<Map<String, Object>> health() {
        return ResponseEntity.ok(Map.of(
                "status", "healthy",
                "service", "spring-visualizer",
                "timestamp", System.currentTimeMillis()
        ));
    }
}
