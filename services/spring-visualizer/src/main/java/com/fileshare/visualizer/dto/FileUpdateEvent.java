package com.fileshare.visualizer.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class FileUpdateEvent {
    private String eventType; // INSERT, UPDATE, DELETE
    private String fileId;
    private String userId;
    private String fileName;
    private String contentType;
    private Long fileSize;
    private String status;
    private LocalDateTime createdAt;
    private LocalDateTime timestamp;
}
