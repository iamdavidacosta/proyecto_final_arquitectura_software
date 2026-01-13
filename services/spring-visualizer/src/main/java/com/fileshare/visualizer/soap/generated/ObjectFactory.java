package com.fileshare.visualizer.soap.generated;

import jakarta.xml.bind.annotation.XmlRegistry;

/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the com.fileshare.visualizer.soap.generated package. 
 */
@XmlRegistry
public class ObjectFactory {

    public ObjectFactory() {
    }

    public GetAllFilesRequest createGetAllFilesRequest() {
        return new GetAllFilesRequest();
    }

    public GetAllFilesResponse createGetAllFilesResponse() {
        return new GetAllFilesResponse();
    }

    public GetFileByIdRequest createGetFileByIdRequest() {
        return new GetFileByIdRequest();
    }

    public GetFileByIdResponse createGetFileByIdResponse() {
        return new GetFileByIdResponse();
    }

    public FileInfo createFileInfo() {
        return new FileInfo();
    }

    public FileDetail createFileDetail() {
        return new FileDetail();
    }

    public MetadataEntry createMetadataEntry() {
        return new MetadataEntry();
    }
}
