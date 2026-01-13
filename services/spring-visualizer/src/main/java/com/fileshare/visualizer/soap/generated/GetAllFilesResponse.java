package com.fileshare.visualizer.soap.generated;

import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlElementWrapper;
import jakarta.xml.bind.annotation.XmlRootElement;
import jakarta.xml.bind.annotation.XmlType;
import java.util.ArrayList;
import java.util.List;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {"files", "totalCount", "pageNumber", "pageSize"})
@XmlRootElement(name = "GetAllFilesResponse", namespace = "http://fileshare.local/fileservice")
public class GetAllFilesResponse {

    @XmlElementWrapper(name = "Files", namespace = "http://fileshare.local/fileservice")
    @XmlElement(name = "FileInfo", namespace = "http://fileshare.local/fileservice")
    private List<FileInfo> files = new ArrayList<>();

    @XmlElement(name = "TotalCount", namespace = "http://fileshare.local/fileservice")
    private int totalCount;

    @XmlElement(name = "PageNumber", namespace = "http://fileshare.local/fileservice")
    private int pageNumber;

    @XmlElement(name = "PageSize", namespace = "http://fileshare.local/fileservice")
    private int pageSize;

    public List<FileInfo> getFiles() {
        return files;
    }

    public void setFiles(List<FileInfo> files) {
        this.files = files;
    }

    public int getTotalCount() {
        return totalCount;
    }

    public void setTotalCount(int totalCount) {
        this.totalCount = totalCount;
    }

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
