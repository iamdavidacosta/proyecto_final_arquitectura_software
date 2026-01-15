package com.fileshare.visualizer.model;

import lombok.Data;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;
import org.springframework.data.mongodb.core.mapping.Field;

import java.time.LocalDateTime;
import java.util.UUID;

@Data
@Document(collection = "file_metadata")
public class FileMetadata {
    @Id
    private String id;

    @Field("fileId")
    private UUID fileId;

    @Field("userId")
    private UUID userId;

    @Field("originalFileName")
    private String originalFileName;

    @Field("contentType")
    private String contentType;

    @Field("fileSize")
    private Long fileSize;

    @Field("hash")
    private String hash;

    @Field("isEncrypted")
    private Boolean isEncrypted;

    @Field("description")
    private String description;

    @Field("status")
    private String status;

    @Field("createdAt")
    private LocalDateTime createdAt;

    @Field("processedAt")
    private LocalDateTime processedAt;

    @Field("minioObjectKey")
    private String minioObjectKey;
}
