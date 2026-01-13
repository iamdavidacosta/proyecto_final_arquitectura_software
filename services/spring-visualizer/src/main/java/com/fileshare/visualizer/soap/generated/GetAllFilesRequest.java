package com.fileshare.visualizer.soap.generated;

import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlRootElement;
import jakarta.xml.bind.annotation.XmlType;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {"pageNumber", "pageSize"})
@XmlRootElement(name = "GetAllFilesRequest", namespace = "http://fileshare.local/fileservice")
public class GetAllFilesRequest {

    @XmlElement(name = "PageNumber", namespace = "http://fileshare.local/fileservice")
    private int pageNumber = 1;

    @XmlElement(name = "PageSize", namespace = "http://fileshare.local/fileservice")
    private int pageSize = 20;

    public int getPageNumber() {
        return pageNumber;
    }

    public void setPageNumber(int pageNumber) {
        this.pageNumber = pageNumber;
    }

    public int getPageSize() {
        return pageSize;
    }

    public void setPageSize(int pageSize) {
        this.pageSize = pageSize;
    }
}
