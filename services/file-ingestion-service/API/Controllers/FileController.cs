using System.Security.Claims;
using FileIngestionService.Application.DTOs;
using FileIngestionService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileIngestionService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileIngestionService _fileIngestionService;
    private readonly ILogger<FileController> _logger;

    public FileController(IFileIngestionService fileIngestionService, ILogger<FileController> logger)
    {
        _fileIngestionService = fileIngestionService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)] // 100 MB limit
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string? description)
    {
        var userId = GetUserId();
        var userEmail = GetUserEmail();

        _logger.LogInformation("File upload request received: {FileName}, Size: {Size}, UserId: {UserId}",
            file.FileName, file.Length, userId);

        var response = await _fileIngestionService.UploadFileAsync(
            userId,
            userEmail,
            file,
            description
        );

        return Ok(response);
    }

    /// <summary>
    /// Get user's files
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FileInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyFiles()
    {
        var userId = GetUserId();
        var files = await _fileIngestionService.GetUserFilesAsync(userId);
        return Ok(files);
    }

    /// <summary>
    /// Get file info by ID
    /// </summary>
    [HttpGet("{fileId:guid}")]
    [ProducesResponseType(typeof(FileInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFileInfo(Guid fileId, CancellationToken cancellationToken = default)
    {
        var fileInfo = await _fileIngestionService.GetFileInfoAsync(fileId, cancellationToken);
        if (fileInfo == null)
        {
            return NotFound();
        }
        return Ok(fileInfo);
    }

    /// <summary>
    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFile(Guid fileId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var deleted = await _fileIngestionService.DeleteFileAsync(fileId, userId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Health check endpoint (public)
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "file-ingestion-service", timestamp = DateTime.UtcNow });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    private string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value ?? "unknown@example.com";
    }
}
