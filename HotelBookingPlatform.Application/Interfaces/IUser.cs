using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces;

/// <summary>
/// Defines a method to associate a user with a guest entity.  
/// This interface helps prevent circular dependencies between  
/// the Application and Infrastructure layers when registering a user.
/// </summary>

public interface IUser
{
    void RegisterUserAsGuest(Guest guest);
}
