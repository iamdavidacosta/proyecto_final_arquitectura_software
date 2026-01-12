# SOAP WSDL Contract

## Descripción
Contrato WSDL para el servicio SOAP de información de archivos procesados.

## Endpoint
```
URL: http://localhost:5003/FileService.svc
WSDL: http://localhost:5003/FileService.svc?wsdl
```

## Autenticación
El servicio SOAP requiere autenticación via JWT en el header SOAP:

```xml
<soapenv:Header>
    <AuthToken xmlns="http://fileshare.local/auth">
        Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    </AuthToken>
</soapenv:Header>
```

## Operaciones

### 1. GetAllFiles
Retorna lista de todos los archivos del usuario autenticado.

**Request:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Header>
        <AuthToken xmlns="http://fileshare.local/auth">Bearer {jwt_token}</AuthToken>
    </soapenv:Header>
    <soapenv:Body>
        <file:GetAllFilesRequest>
            <file:PageNumber>1</file:PageNumber>
            <file:PageSize>20</file:PageSize>
        </file:GetAllFilesRequest>
    </soapenv:Body>
</soapenv:Envelope>
```

**Response:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Body>
        <file:GetAllFilesResponse>
            <file:Files>
                <file:FileInfo>
                    <file:FileId>550e8400-e29b-41d4-a716-446655440000</file:FileId>
                    <file:FileName>document.pdf</file:FileName>
                    <file:FileSize>1048576</file:FileSize>
                    <file:ContentType>application/pdf</file:ContentType>
                    <file:Status>UploadedToMinIO</file:Status>
                    <file:UploadedAt>2024-01-15T10:30:00Z</file:UploadedAt>
                    <file:Sha256Hash>e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855</file:Sha256Hash>
                    <file:UserId>7c9e6679-7425-40de-944b-e07fc1f90ae7</file:UserId>
                    <file:UserEmail>john.doe@example.com</file:UserEmail>
                </file:FileInfo>
            </file:Files>
            <file:TotalCount>1</file:TotalCount>
            <file:PageNumber>1</file:PageNumber>
            <file:PageSize>20</file:PageSize>
        </file:GetAllFilesResponse>
    </soapenv:Body>
</soapenv:Envelope>
```

### 2. GetFileById
Retorna detalle completo de un archivo específico.

**Request:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Header>
        <AuthToken xmlns="http://fileshare.local/auth">Bearer {jwt_token}</AuthToken>
    </soapenv:Header>
    <soapenv:Body>
        <file:GetFileByIdRequest>
            <file:FileId>550e8400-e29b-41d4-a716-446655440000</file:FileId>
        </file:GetFileByIdRequest>
    </soapenv:Body>
</soapenv:Envelope>
```

**Response:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Body>
        <file:GetFileByIdResponse>
            <file:File>
                <file:FileId>550e8400-e29b-41d4-a716-446655440000</file:FileId>
                <file:FileName>document.pdf</file:FileName>
                <file:FileSize>1048576</file:FileSize>
                <file:ContentType>application/pdf</file:ContentType>
                <file:Status>UploadedToMinIO</file:Status>
                <file:UploadedAt>2024-01-15T10:30:00Z</file:UploadedAt>
                <file:ProcessedAt>2024-01-15T10:31:00Z</file:ProcessedAt>
                <file:Sha256Hash>e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855</file:Sha256Hash>
                <file:UserId>7c9e6679-7425-40de-944b-e07fc1f90ae7</file:UserId>
                <file:UserEmail>john.doe@example.com</file:UserEmail>
                <file:OriginalMinioPath>original-files/550e8400.../document.pdf</file:OriginalMinioPath>
                <file:EncryptedMinioPath>encrypted-files/550e8400.../document.pdf.enc</file:EncryptedMinioPath>
                <file:Metadata>
                    <file:Entry>
                        <file:Key>author</file:Key>
                        <file:Value>John Doe</file:Value>
                    </file:Entry>
                    <file:Entry>
                        <file:Key>pageCount</file:Key>
                        <file:Value>10</file:Value>
                    </file:Entry>
                </file:Metadata>
            </file:File>
        </file:GetFileByIdResponse>
    </soapenv:Body>
</soapenv:Envelope>
```

### 3. GetPipelineStatusById
Retorna estado detallado del pipeline de procesamiento.

**Request:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Header>
        <AuthToken xmlns="http://fileshare.local/auth">Bearer {jwt_token}</AuthToken>
    </soapenv:Header>
    <soapenv:Body>
        <file:GetPipelineStatusByIdRequest>
            <file:FileId>550e8400-e29b-41d4-a716-446655440000</file:FileId>
        </file:GetPipelineStatusByIdRequest>
    </soapenv:Body>
</soapenv:Envelope>
```

**Response:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Body>
        <file:GetPipelineStatusByIdResponse>
            <file:PipelineStatus>
                <file:FileId>550e8400-e29b-41d4-a716-446655440000</file:FileId>
                <file:CurrentStatus>UploadedToMinIO</file:CurrentStatus>
                <file:Stages>
                    <file:Stage>
                        <file:Name>MetadataExtraction</file:Name>
                        <file:Status>Completed</file:Status>
                        <file:StartedAt>2024-01-15T10:30:05Z</file:StartedAt>
                        <file:CompletedAt>2024-01-15T10:30:10Z</file:CompletedAt>
                    </file:Stage>
                    <file:Stage>
                        <file:Name>HashGeneration</file:Name>
                        <file:Status>Completed</file:Status>
                        <file:StartedAt>2024-01-15T10:30:10Z</file:StartedAt>
                        <file:CompletedAt>2024-01-15T10:30:20Z</file:CompletedAt>
                    </file:Stage>
                    <file:Stage>
                        <file:Name>Encryption</file:Name>
                        <file:Status>Completed</file:Status>
                        <file:StartedAt>2024-01-15T10:30:20Z</file:StartedAt>
                        <file:CompletedAt>2024-01-15T10:30:40Z</file:CompletedAt>
                    </file:Stage>
                    <file:Stage>
                        <file:Name>DecryptionValidation</file:Name>
                        <file:Status>Completed</file:Status>
                        <file:StartedAt>2024-01-15T10:30:40Z</file:StartedAt>
                        <file:CompletedAt>2024-01-15T10:30:50Z</file:CompletedAt>
                    </file:Stage>
                    <file:Stage>
                        <file:Name>MinIOUpload</file:Name>
                        <file:Status>Completed</file:Status>
                        <file:StartedAt>2024-01-15T10:30:50Z</file:StartedAt>
                        <file:CompletedAt>2024-01-15T10:31:00Z</file:CompletedAt>
                    </file:Stage>
                </file:Stages>
            </file:PipelineStatus>
        </file:GetPipelineStatusByIdResponse>
    </soapenv:Body>
</soapenv:Envelope>
```

## WSDL Completo

```xml
<?xml version="1.0" encoding="UTF-8"?>
<wsdl:definitions xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/"
                  xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/"
                  xmlns:tns="http://fileshare.local/fileservice"
                  xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                  name="FileService"
                  targetNamespace="http://fileshare.local/fileservice">

    <!-- Types -->
    <wsdl:types>
        <xsd:schema targetNamespace="http://fileshare.local/fileservice">
            
            <!-- Simple Types -->
            <xsd:simpleType name="FileStatusType">
                <xsd:restriction base="xsd:string">
                    <xsd:enumeration value="Received"/>
                    <xsd:enumeration value="MetadataStored"/>
                    <xsd:enumeration value="Hashed"/>
                    <xsd:enumeration value="Encrypted"/>
                    <xsd:enumeration value="DecryptedValidated"/>
                    <xsd:enumeration value="UploadedToMinIO"/>
                    <xsd:enumeration value="Failed"/>
                </xsd:restriction>
            </xsd:simpleType>
            
            <xsd:simpleType name="StageStatusType">
                <xsd:restriction base="xsd:string">
                    <xsd:enumeration value="Pending"/>
                    <xsd:enumeration value="InProgress"/>
                    <xsd:enumeration value="Completed"/>
                    <xsd:enumeration value="Failed"/>
                    <xsd:enumeration value="Skipped"/>
                </xsd:restriction>
            </xsd:simpleType>

            <!-- Complex Types -->
            <xsd:complexType name="MetadataEntry">
                <xsd:sequence>
                    <xsd:element name="Key" type="xsd:string"/>
                    <xsd:element name="Value" type="xsd:string"/>
                </xsd:sequence>
            </xsd:complexType>

            <xsd:complexType name="FileInfo">
                <xsd:sequence>
                    <xsd:element name="FileId" type="xsd:string"/>
                    <xsd:element name="FileName" type="xsd:string"/>
                    <xsd:element name="FileSize" type="xsd:long"/>
                    <xsd:element name="ContentType" type="xsd:string"/>
                    <xsd:element name="Status" type="tns:FileStatusType"/>
                    <xsd:element name="UploadedAt" type="xsd:dateTime"/>
                    <xsd:element name="ProcessedAt" type="xsd:dateTime" minOccurs="0"/>
                    <xsd:element name="Sha256Hash" type="xsd:string" minOccurs="0"/>
                    <xsd:element name="UserId" type="xsd:string"/>
                    <xsd:element name="UserEmail" type="xsd:string"/>
                    <xsd:element name="OriginalMinioPath" type="xsd:string" minOccurs="0"/>
                    <xsd:element name="EncryptedMinioPath" type="xsd:string" minOccurs="0"/>
                    <xsd:element name="Metadata" minOccurs="0">
                        <xsd:complexType>
                            <xsd:sequence>
                                <xsd:element name="Entry" type="tns:MetadataEntry" maxOccurs="unbounded"/>
                            </xsd:sequence>
                        </xsd:complexType>
                    </xsd:element>
                </xsd:sequence>
            </xsd:complexType>

            <xsd:complexType name="PipelineStage">
                <xsd:sequence>
                    <xsd:element name="Name" type="xsd:string"/>
                    <xsd:element name="Status" type="tns:StageStatusType"/>
                    <xsd:element name="StartedAt" type="xsd:dateTime" minOccurs="0"/>
                    <xsd:element name="CompletedAt" type="xsd:dateTime" minOccurs="0"/>
                    <xsd:element name="Error" type="xsd:string" minOccurs="0"/>
                </xsd:sequence>
            </xsd:complexType>

            <xsd:complexType name="PipelineStatus">
                <xsd:sequence>
                    <xsd:element name="FileId" type="xsd:string"/>
                    <xsd:element name="CurrentStatus" type="tns:FileStatusType"/>
                    <xsd:element name="Stages">
                        <xsd:complexType>
                            <xsd:sequence>
                                <xsd:element name="Stage" type="tns:PipelineStage" maxOccurs="unbounded"/>
                            </xsd:sequence>
                        </xsd:complexType>
                    </xsd:element>
                </xsd:sequence>
            </xsd:complexType>

            <!-- Request/Response Elements -->
            <xsd:element name="GetAllFilesRequest">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="PageNumber" type="xsd:int" default="1"/>
                        <xsd:element name="PageSize" type="xsd:int" default="20"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

            <xsd:element name="GetAllFilesResponse">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="Files">
                            <xsd:complexType>
                                <xsd:sequence>
                                    <xsd:element name="FileInfo" type="tns:FileInfo" maxOccurs="unbounded" minOccurs="0"/>
                                </xsd:sequence>
                            </xsd:complexType>
                        </xsd:element>
                        <xsd:element name="TotalCount" type="xsd:int"/>
                        <xsd:element name="PageNumber" type="xsd:int"/>
                        <xsd:element name="PageSize" type="xsd:int"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

            <xsd:element name="GetFileByIdRequest">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="FileId" type="xsd:string"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

            <xsd:element name="GetFileByIdResponse">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="File" type="tns:FileInfo" minOccurs="0"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

            <xsd:element name="GetPipelineStatusByIdRequest">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="FileId" type="xsd:string"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

            <xsd:element name="GetPipelineStatusByIdResponse">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="PipelineStatus" type="tns:PipelineStatus" minOccurs="0"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

            <!-- Fault -->
            <xsd:element name="ServiceFault">
                <xsd:complexType>
                    <xsd:sequence>
                        <xsd:element name="ErrorCode" type="xsd:string"/>
                        <xsd:element name="ErrorMessage" type="xsd:string"/>
                    </xsd:sequence>
                </xsd:complexType>
            </xsd:element>

        </xsd:schema>
    </wsdl:types>

    <!-- Messages -->
    <wsdl:message name="GetAllFilesRequestMessage">
        <wsdl:part name="parameters" element="tns:GetAllFilesRequest"/>
    </wsdl:message>
    <wsdl:message name="GetAllFilesResponseMessage">
        <wsdl:part name="parameters" element="tns:GetAllFilesResponse"/>
    </wsdl:message>

    <wsdl:message name="GetFileByIdRequestMessage">
        <wsdl:part name="parameters" element="tns:GetFileByIdRequest"/>
    </wsdl:message>
    <wsdl:message name="GetFileByIdResponseMessage">
        <wsdl:part name="parameters" element="tns:GetFileByIdResponse"/>
    </wsdl:message>

    <wsdl:message name="GetPipelineStatusByIdRequestMessage">
        <wsdl:part name="parameters" element="tns:GetPipelineStatusByIdRequest"/>
    </wsdl:message>
    <wsdl:message name="GetPipelineStatusByIdResponseMessage">
        <wsdl:part name="parameters" element="tns:GetPipelineStatusByIdResponse"/>
    </wsdl:message>

    <wsdl:message name="ServiceFaultMessage">
        <wsdl:part name="fault" element="tns:ServiceFault"/>
    </wsdl:message>

    <!-- Port Type -->
    <wsdl:portType name="IFileService">
        <wsdl:operation name="GetAllFiles">
            <wsdl:input message="tns:GetAllFilesRequestMessage"/>
            <wsdl:output message="tns:GetAllFilesResponseMessage"/>
            <wsdl:fault name="fault" message="tns:ServiceFaultMessage"/>
        </wsdl:operation>

        <wsdl:operation name="GetFileById">
            <wsdl:input message="tns:GetFileByIdRequestMessage"/>
            <wsdl:output message="tns:GetFileByIdResponseMessage"/>
            <wsdl:fault name="fault" message="tns:ServiceFaultMessage"/>
        </wsdl:operation>

        <wsdl:operation name="GetPipelineStatusById">
            <wsdl:input message="tns:GetPipelineStatusByIdRequestMessage"/>
            <wsdl:output message="tns:GetPipelineStatusByIdResponseMessage"/>
            <wsdl:fault name="fault" message="tns:ServiceFaultMessage"/>
        </wsdl:operation>
    </wsdl:portType>

    <!-- Binding -->
    <wsdl:binding name="FileServiceSoapBinding" type="tns:IFileService">
        <soap:binding style="document" transport="http://schemas.xmlsoap.org/soap/http"/>
        
        <wsdl:operation name="GetAllFiles">
            <soap:operation soapAction="http://fileshare.local/fileservice/GetAllFiles"/>
            <wsdl:input>
                <soap:body use="literal"/>
            </wsdl:input>
            <wsdl:output>
                <soap:body use="literal"/>
            </wsdl:output>
            <wsdl:fault name="fault">
                <soap:fault name="fault" use="literal"/>
            </wsdl:fault>
        </wsdl:operation>

        <wsdl:operation name="GetFileById">
            <soap:operation soapAction="http://fileshare.local/fileservice/GetFileById"/>
            <wsdl:input>
                <soap:body use="literal"/>
            </wsdl:input>
            <wsdl:output>
                <soap:body use="literal"/>
            </wsdl:output>
            <wsdl:fault name="fault">
                <soap:fault name="fault" use="literal"/>
            </wsdl:fault>
        </wsdl:operation>

        <wsdl:operation name="GetPipelineStatusById">
            <soap:operation soapAction="http://fileshare.local/fileservice/GetPipelineStatusById"/>
            <wsdl:input>
                <soap:body use="literal"/>
            </wsdl:input>
            <wsdl:output>
                <soap:body use="literal"/>
            </wsdl:output>
            <wsdl:fault name="fault">
                <soap:fault name="fault" use="literal"/>
            </wsdl:fault>
        </wsdl:operation>
    </wsdl:binding>

    <!-- Service -->
    <wsdl:service name="FileService">
        <wsdl:port name="FileServiceSoapPort" binding="tns:FileServiceSoapBinding">
            <soap:address location="http://localhost:5003/FileService.svc"/>
        </wsdl:port>
    </wsdl:service>

</wsdl:definitions>
```

## Códigos de Error SOAP

| Código | Descripción |
|--------|-------------|
| `AUTH_001` | Token no proporcionado |
| `AUTH_002` | Token inválido o expirado |
| `AUTH_003` | Permisos insuficientes |
| `FILE_001` | Archivo no encontrado |
| `FILE_002` | Error al acceder al archivo |
| `SYS_001` | Error interno del sistema |

## Ejemplo de Fault Response

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:file="http://fileshare.local/fileservice">
    <soapenv:Body>
        <soapenv:Fault>
            <faultcode>soapenv:Client</faultcode>
            <faultstring>Authentication Failed</faultstring>
            <detail>
                <file:ServiceFault>
                    <file:ErrorCode>AUTH_002</file:ErrorCode>
                    <file:ErrorMessage>Token has expired</file:ErrorMessage>
                </file:ServiceFault>
            </detail>
        </soapenv:Fault>
    </soapenv:Body>
</soapenv:Envelope>
```
