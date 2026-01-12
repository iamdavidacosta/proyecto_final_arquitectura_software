using System.Runtime.Serialization;

namespace SoapService.Contracts;

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class FileInfo
{
    [DataMember(Order = 1)]
    public string FileId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string UserId { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string FileName { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string ContentType { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public long FileSize { get; set; }

    [DataMember(Order = 6)]
    public string Hash { get; set; } = string.Empty;

    [DataMember(Order = 7)]
    public bool IsEncrypted { get; set; }

    [DataMember(Order = 8)]
    public string? Description { get; set; }

    [DataMember(Order = 9)]
    public string Status { get; set; } = string.Empty;

    [DataMember(Order = 10)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 11)]
    public DateTime? ProcessedAt { get; set; }
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class GetFileRequest
{
    [DataMember(Order = 1)]
    public string FileId { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class GetFileResponse
{
    [DataMember(Order = 1)]
    public bool Success { get; set; }

    [DataMember(Order = 2)]
    public string? ErrorMessage { get; set; }

    [DataMember(Order = 3)]
    public FileInfo? File { get; set; }
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class GetUserFilesRequest
{
    [DataMember(Order = 1)]
    public string UserId { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class GetUserFilesResponse
{
    [DataMember(Order = 1)]
    public bool Success { get; set; }

    [DataMember(Order = 2)]
    public string? ErrorMessage { get; set; }

    [DataMember(Order = 3)]
    public List<FileInfo> Files { get; set; } = new();
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class GetDownloadUrlRequest
{
    [DataMember(Order = 1)]
    public string FileId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public int ExpiryInSeconds { get; set; } = 3600;
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class GetDownloadUrlResponse
{
    [DataMember(Order = 1)]
    public bool Success { get; set; }

    [DataMember(Order = 2)]
    public string? ErrorMessage { get; set; }

    [DataMember(Order = 3)]
    public string? DownloadUrl { get; set; }

    [DataMember(Order = 4)]
    public DateTime? ExpiresAt { get; set; }
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class DeleteFileRequest
{
    [DataMember(Order = 1)]
    public string FileId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string UserId { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://fileshare.com/soap/files")]
public class DeleteFileResponse
{
    [DataMember(Order = 1)]
    public bool Success { get; set; }

    [DataMember(Order = 2)]
    public string? ErrorMessage { get; set; }
}
