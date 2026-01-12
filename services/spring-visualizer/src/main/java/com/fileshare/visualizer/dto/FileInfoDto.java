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
public class FileInfoDto {
    private String fileId;
    private String userId;
    private String fileName;
    private String contentType;
    private Long fileSize;
    private String hash;
    private Boolean isEncrypted;
    private String description;
    private String status;
    private LocalDateTime createdAt;
    private LocalDateTime processedAt;
}
