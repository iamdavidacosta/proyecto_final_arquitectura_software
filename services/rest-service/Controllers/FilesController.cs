using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestService.Application.Services;
using RestService.DTOs;

namespace RestService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of user's files
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(FileListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyFiles([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var response = await _fileService.GetUserFilesAsync(userId, page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get a specific file info
    /// </summary>
    [HttpGet("{fileId:guid}")]
    [ProducesResponseType(typeof(FileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(Guid fileId)
    {
        var userId = GetUserId();
        var file = await _fileService.GetFileAsync(fileId, userId);
        
        if (file == null)
            return NotFound(new { error = "File not found" });
        
        return Ok(file);
    }

    /// <summary>
    /// Get download URL for a file
    /// </summary>
    [HttpGet("{fileId:guid}/download")]
    [ProducesResponseType(typeof(FileDownloadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDownloadUrl(Guid fileId)
    {
        var userId = GetUserId();
        
        try
        {
            var response = await _fileService.GetDownloadUrlAsync(fileId, userId);
            return Ok(response);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get files shared with the user
    /// </summary>
    [HttpGet("shared")]
    [ProducesResponseType(typeof(IEnumerable<FileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSharedFiles()
    {
        var userId = GetUserId();
        var files = await _fileService.GetSharedFilesAsync(userId);
        return Ok(files);
    }

    /// <summary>
    /// Share a file with another user
    /// </summary>
    [HttpPost("{fileId:guid}/share")]
    [ProducesResponseType(typeof(ShareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ShareFile(Guid fileId, [FromBody] ShareFileRequest request)
    {
        var userId = GetUserId();
        
        try
        {
            var response = await _fileService.ShareFileAsync(fileId, userId, request);
            return Ok(response);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        var userId = GetUserId();
        
        try
        {
            await _fileService.DeleteFileAsync(fileId, userId);
            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown";
        return Ok(new { 
            status = "healthy", 
            service = "rest-service",
            instance = hostname,
            timestamp = DateTime.UtcNow 
        });
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
}
