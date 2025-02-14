using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;
    public BookingRepository(HotelBookingPlatformDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Booking?> GetBookingAsync(Guid bookingId)
    {
        return await _dbContext.Bookings
            .AsSplitQuery()
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r.Hotel)
                       .ThenInclude(h => h.City)
            .Include(b => b.BookingRooms)
                .ThenInclude(r => r.Discount)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<Booking> AddBookingAsync(Booking booking)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Booking> entry = await _dbContext.Bookings.AddAsync(booking);
        return entry.Entity;
    }

    public async Task<bool> DeleteBookingAsync(Guid id)
    {
        Booking? booking = await _dbContext.Bookings.FindAsync(id);
        if (booking is null)
        {
            return false;
        }

        _dbContext.Bookings.Remove(booking);
        return true;
    }

    public async Task BeginTransactionAsync()
    {
        await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? currentTransaction = _dbContext.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            await _dbContext.Database.CommitTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? currentTransaction = _dbContext.Database.CurrentTransaction;
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
