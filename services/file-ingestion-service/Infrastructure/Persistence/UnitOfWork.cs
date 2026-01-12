using FileIngestionService.Domain.Interfaces;
using FileIngestionService.Infrastructure.Persistence.Repositories;

namespace FileIngestionService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly FileDbContext _context;
    private IFileRepository? _files;

    public UnitOfWork(FileDbContext context)
    {
        _context = context;
    }

    public IFileRepository Files => _files ??= new FileRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
