package com.fileshare.visualizer.soap.generated;

import jakarta.xml.bind.annotation.XmlAccessType;
import jakarta.xml.bind.annotation.XmlAccessorType;
import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlType;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "MetadataEntry", namespace = "http://fileshare.local/fileservice", propOrder = {"key", "value"})
public class MetadataEntry {

    @XmlElement(name = "Key", namespace = "http://fileshare.local/fileservice")
    private String key;

    @XmlElement(name = "Value", namespace = "http://fileshare.local/fileservice")
    private String value;

    public String getKey() {
        return key;
    }

    public void setKey(String key) {
        this.key = key;
    }

    public String getValue() {
        return value;
    }

    public void setValue(String value) {
        this.value = value;
    }
}
