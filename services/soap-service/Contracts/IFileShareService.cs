using System.ServiceModel;
using SoapService.Contracts;

namespace SoapService.Contracts;

[ServiceContract(Namespace = "http://fileshare.com/soap/files")]
public interface IFileShareService
{
    [OperationContract]
    Task<GetFileResponse> GetFileAsync(GetFileRequest request);

    [OperationContract]
    Task<GetUserFilesResponse> GetUserFilesAsync(GetUserFilesRequest request);

    [OperationContract]
    Task<GetDownloadUrlResponse> GetDownloadUrlAsync(GetDownloadUrlRequest request);

    [OperationContract]
    Task<DeleteFileResponse> DeleteFileAsync(DeleteFileRequest request);
}
