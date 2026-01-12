using System.Security.Claims;
using FileIngestionService.Application.DTOs;
using FileIngestionService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FileIngestionService.API.Hubs;

[Authorize]
public class FileUploadHub : Hub
{
    private readonly IFileIngestionService _fileIngestionService;
    private readonly ILogger<FileUploadHub> _logger;

    public FileUploadHub(IFileIngestionService fileIngestionService, ILogger<FileUploadHub> logger)
    {
        _fileIngestionService = fileIngestionService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation("Client connected: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _logger.LogInformation("Client disconnected: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<FileUploadResponse> UploadFile(byte[] fileData, string fileName, string contentType, string? description)
    {
        var userId = GetUserId();
        var userEmail = GetUserEmail();

        _logger.LogInformation("Received file upload via SignalR: {FileName}, Size: {Size}, UserId: {UserId}", 
            fileName, fileData.Length, userId);

        // Create IFormFile from byte array
        var stream = new MemoryStream(fileData);
        var formFile = new FormFile(stream, 0, fileData.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

        var response = await _fileIngestionService.UploadFileAsync(
            userId,
            userEmail,
            formFile,
            description,
            async progress =>
            {
                await Clients.Caller.SendAsync("UploadProgress", progress);
            }
        );

        return response;
    }

    public async Task<IEnumerable<FileInfoDto>> GetMyFiles()
    {
        var userId = GetUserId();
        return await _fileIngestionService.GetUserFilesAsync(userId);
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier) 
            ?? Context.User?.FindFirst("sub");
        
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new HubException("User not authenticated");
        }

        return userId;
    }

    private string GetUserEmail()
    {
        return Context.User?.FindFirst(ClaimTypes.Email)?.Value 
            ?? Context.User?.FindFirst("email")?.Value 
            ?? "unknown@example.com";
    }
}
