package com.fileshare.visualizer.soap.generated;

import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlType;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "FileInfo", namespace = "http://fileshare.local/fileservice", propOrder = {
    "fileId", "fileName", "fileSize", "contentType", "status", "uploadedAt", "sha256Hash", "userId", "userEmail"
})
public class FileInfo {

    @XmlElement(name = "FileId", namespace = "http://fileshare.local/fileservice")
    private String fileId;

    @XmlElement(name = "FileName", namespace = "http://fileshare.local/fileservice")
    private String fileName;

    @XmlElement(name = "FileSize", namespace = "http://fileshare.local/fileservice")
    private long fileSize;

    @XmlElement(name = "ContentType", namespace = "http://fileshare.local/fileservice")
    private String contentType;

    @XmlElement(name = "Status", namespace = "http://fileshare.local/fileservice")
    private String status;

    @XmlElement(name = "UploadedAt", namespace = "http://fileshare.local/fileservice")
    private String uploadedAt;

    @XmlElement(name = "Sha256Hash", namespace = "http://fileshare.local/fileservice")
    private String sha256Hash;

    @XmlElement(name = "UserId", namespace = "http://fileshare.local/fileservice")
    private String userId;

    @XmlElement(name = "UserEmail", namespace = "http://fileshare.local/fileservice")
    private String userEmail;

    public String getFileId() {
        return fileId;
    }

    public void setFileId(String fileId) {
        this.fileId = fileId;
    }

    public String getFileName() {
        return fileName;
    }

    public void setFileName(String fileName) {
        this.fileName = fileName;
    }

    public long getFileSize() {
        return fileSize;
    }

    public void setFileSize(long fileSize) {
        this.fileSize = fileSize;
    }

    public String getContentType() {
        return contentType;
    }

    public void setContentType(String contentType) {
        this.contentType = contentType;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getUploadedAt() {
        return uploadedAt;
    }

    public void setUploadedAt(String uploadedAt) {
        this.uploadedAt = uploadedAt;
    }

    public String getSha256Hash() {
        return sha256Hash;
    }

    public void setSha256Hash(String sha256Hash) {
        this.sha256Hash = sha256Hash;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public String getUserEmail() {
        return userEmail;
    }

    public void setUserEmail(String userEmail) {
        this.userEmail = userEmail;
    }
}
