using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingPlatform.Infrastructure.Identity;

public class ApplicationUser : IdentityUser, IUser
{
    public ApplicationUser(string userName, string email) : base(userName: userName)
    {
        Email = email;
    }

    public Guest? Guest { get; private set; }
    public void RegisterUserAsGuest(Guest guest)
    {
        Guest = guest;
    }
}
