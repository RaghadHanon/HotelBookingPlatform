using HotelBookingPlatform.Application.Interfaces.Infrastructure;

namespace HotelBookingPlatform.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public UnitOfWork(HotelBookingPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync()
    {
        await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        var currentTransaction = _dbContext.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            await _dbContext.Database.CommitTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        var currentTransaction = _dbContext.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}
