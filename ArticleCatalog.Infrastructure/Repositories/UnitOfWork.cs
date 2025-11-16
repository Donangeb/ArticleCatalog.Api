using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ArticleCatalog.Infrastructure.Repositories;

/// <summary>
/// Реализация Unit of Work для управления транзакциями
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ArticleCatalogDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ArticleCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}

