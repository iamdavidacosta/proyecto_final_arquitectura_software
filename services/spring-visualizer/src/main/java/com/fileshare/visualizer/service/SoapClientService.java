package com.fileshare.visualizer.service;

import com.fileshare.visualizer.dto.DownloadUrlDto;
import com.fileshare.visualizer.dto.FileInfoDto;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import java.io.ByteArrayInputStream;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;

@Slf4j
@Service
public class SoapClientService {

    @Value("${soap.client.url}")
    private String soapUrl;

    private final RestTemplate restTemplate;

    public SoapClientService() {
        this.restTemplate = new RestTemplate();
    }

    public FileInfoDto getFile(String fileId) {
        log.info("Getting file info via SOAP for fileId: {}", fileId);

        String soapRequest = buildGetFileRequest(fileId);
        String response = sendSoapRequest(soapRequest);
        
        return parseGetFileResponse(response);
    }

    public List<FileInfoDto> getUserFiles(String userId) {
        log.info("Getting user files via SOAP for userId: {}", userId);

        String soapRequest = buildGetUserFilesRequest(userId);
        String response = sendSoapRequest(soapRequest);
        
        return parseGetUserFilesResponse(response);
    }

    public DownloadUrlDto getDownloadUrl(String fileId, int expiryInSeconds) {
        log.info("Getting download URL via SOAP for fileId: {}", fileId);

        String soapRequest = buildGetDownloadUrlRequest(fileId, expiryInSeconds);
        String response = sendSoapRequest(soapRequest);
        
        return parseGetDownloadUrlResponse(response);
    }

    public boolean deleteFile(String fileId, String userId) {
        log.info("Deleting file via SOAP for fileId: {}, userId: {}", fileId, userId);

        String soapRequest = buildDeleteFileRequest(fileId, userId);
        String response = sendSoapRequest(soapRequest);
        
        return parseDeleteFileResponse(response);
    }

    private String sendSoapRequest(String soapRequest) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.TEXT_XML);
        headers.set("SOAPAction", "");

        HttpEntity<String> entity = new HttpEntity<>(soapRequest, headers);

        ResponseEntity<String> response = restTemplate.exchange(
                soapUrl,
                HttpMethod.POST,
                entity,
                String.class
        );

        log.debug("SOAP Response: {}", response.getBody());
        return response.getBody();
    }

    private String buildGetFileRequest(String fileId) {
        return String.format("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                           xmlns:files="http://fileshare.com/soap/files">
                <soap:Body>
                    <files:GetFileRequest>
                        <files:FileId>%s</files:FileId>
                    </files:GetFileRequest>
                </soap:Body>
            </soap:Envelope>
            """, fileId);
    }

    private String buildGetUserFilesRequest(String userId) {
        return String.format("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                           xmlns:files="http://fileshare.com/soap/files">
                <soap:Body>
                    <files:GetUserFilesRequest>
                        <files:UserId>%s</files:UserId>
                    </files:GetUserFilesRequest>
                </soap:Body>
            </soap:Envelope>
            """, userId);
    }

    private String buildGetDownloadUrlRequest(String fileId, int expiryInSeconds) {
        return String.format("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                           xmlns:files="http://fileshare.com/soap/files">
                <soap:Body>
                    <files:GetDownloadUrlRequest>
                        <files:FileId>%s</files:FileId>
                        <files:ExpiryInSeconds>%d</files:ExpiryInSeconds>
                    </files:GetDownloadUrlRequest>
                </soap:Body>
            </soap:Envelope>
            """, fileId, expiryInSeconds);
    }

    private String buildDeleteFileRequest(String fileId, String userId) {
        return String.format("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                           xmlns:files="http://fileshare.com/soap/files">
                <soap:Body>
                    <files:DeleteFileRequest>
                        <files:FileId>%s</files:FileId>
                        <files:UserId>%s</files:UserId>
                    </files:DeleteFileRequest>
                </soap:Body>
            </soap:Envelope>
            """, fileId, userId);
    }

    private FileInfoDto parseGetFileResponse(String xml) {
        try {
            Document doc = parseXml(xml);
            Element fileElement = (Element) doc.getElementsByTagName("File").item(0);
            
            if (fileElement == null) {
                return null;
            }

            return mapToFileInfoDto(fileElement);
        } catch (Exception e) {
            log.error("Error parsing GetFile response", e);
            return null;
        }
    }

    private List<FileInfoDto> parseGetUserFilesResponse(String xml) {
        List<FileInfoDto> files = new ArrayList<>();
        
        try {
            Document doc = parseXml(xml);
            NodeList fileNodes = doc.getElementsByTagName("Files");
            
            for (int i = 0; i < fileNodes.getLength(); i++) {
                Element fileElement = (Element) fileNodes.item(i);
                files.add(mapToFileInfoDto(fileElement));
            }
        } catch (Exception e) {
            log.error("Error parsing GetUserFiles response", e);
        }
        
        return files;
    }

    private DownloadUrlDto parseGetDownloadUrlResponse(String xml) {
        try {
            Document doc = parseXml(xml);
            
            String downloadUrl = getElementText(doc.getDocumentElement(), "DownloadUrl");
            String expiresAt = getElementText(doc.getDocumentElement(), "ExpiresAt");

            return DownloadUrlDto.builder()
                    .downloadUrl(downloadUrl)
                    .expiresAt(parseDateTime(expiresAt))
                    .build();
        } catch (Exception e) {
            log.error("Error parsing GetDownloadUrl response", e);
            return null;
        }
    }

    private boolean parseDeleteFileResponse(String xml) {
        try {
            Document doc = parseXml(xml);
            String success = getElementText(doc.getDocumentElement(), "Success");
            return "true".equalsIgnoreCase(success);
        } catch (Exception e) {
            log.error("Error parsing DeleteFile response", e);
            return false;
        }
    }

    private FileInfoDto mapToFileInfoDto(Element element) {
        return FileInfoDto.builder()
                .fileId(getElementText(element, "FileId"))
                .userId(getElementText(element, "UserId"))
                .fileName(getElementText(element, "FileName"))
                .contentType(getElementText(element, "ContentType"))
                .fileSize(parseLong(getElementText(element, "FileSize")))
                .hash(getElementText(element, "Hash"))
                .isEncrypted(parseBoolean(getElementText(element, "IsEncrypted")))
                .description(getElementText(element, "Description"))
                .status(getElementText(element, "Status"))
                .createdAt(parseDateTime(getElementText(element, "CreatedAt")))
                .processedAt(parseDateTime(getElementText(element, "ProcessedAt")))
                .build();
    }

    private Document parseXml(String xml) throws Exception {
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        DocumentBuilder builder = factory.newDocumentBuilder();
        return builder.parse(new ByteArrayInputStream(xml.getBytes(StandardCharsets.UTF_8)));
    }

    private String getElementText(Element parent, String tagName) {
        NodeList nodes = parent.getElementsByTagName(tagName);
        if (nodes.getLength() > 0) {
            return nodes.item(0).getTextContent();
        }
        return null;
    }

    private Long parseLong(String value) {
        try {
            return value != null ? Long.parseLong(value) : null;
        } catch (NumberFormatException e) {
            return null;
        }
    }

    private Boolean parseBoolean(String value) {
        return value != null ? Boolean.parseBoolean(value) : null;
    }

    private LocalDateTime parseDateTime(String value) {
        if (value == null || value.isEmpty()) {
            return null;
        }
        try {
            return LocalDateTime.parse(value, DateTimeFormatter.ISO_DATE_TIME);
        } catch (Exception e) {
            return null;
        }
    }
}
