using FileIngestionService.Domain.Entities;

namespace FileIngestionService.Domain.Interfaces;

public interface IFileRepository
{
    Task<FileRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(FileRecord file, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileRecord file, CancellationToken cancellationToken = default);
}
