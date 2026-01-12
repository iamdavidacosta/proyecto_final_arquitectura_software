namespace FileIngestionService.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IFileRepository Files { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
