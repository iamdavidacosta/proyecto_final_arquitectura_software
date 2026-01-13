package com.fileshare.visualizer.soap.generated;

import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlRootElement;
import jakarta.xml.bind.annotation.XmlType;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {"file"})
@XmlRootElement(name = "GetFileByIdResponse", namespace = "http://fileshare.local/fileservice")
public class GetFileByIdResponse {

    @XmlElement(name = "File", namespace = "http://fileshare.local/fileservice")
    private FileDetail file;

    public FileDetail getFile() {
        return file;
    }

    public void setFile(FileDetail file) {
        this.file = file;
    }
}
