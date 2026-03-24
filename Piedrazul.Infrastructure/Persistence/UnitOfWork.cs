using Microsoft.EntityFrameworkCore.Storage;
using Piedrazul.Application.Common.Interfaces;

namespace Piedrazul.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PiedrazulDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
