using FileIngestionService.Domain.Entities;
using FileIngestionService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileIngestionService.Infrastructure.Persistence.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FileDbContext _context;

    public FileRepository(FileDbContext context)
    {
        _context = context;
    }

    public async Task<FileRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<FileRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FileRecord file, CancellationToken cancellationToken = default)
    {
        await _context.Files.AddAsync(file, cancellationToken);
    }

    public Task UpdateAsync(FileRecord file, CancellationToken cancellationToken = default)
    {
        _context.Files.Update(file);
        return Task.CompletedTask;
    }
}
