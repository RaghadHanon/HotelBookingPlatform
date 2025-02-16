namespace HotelBookingPlatform.Application.Interfaces.Infrastructure;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<bool> SaveChangesAsync();
}
