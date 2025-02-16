using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Identity;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityManager _identityManager;
    private readonly IGuestRepository _guestRepository;

    public AuthenticationService(IIdentityManager identityManager, IGuestRepository guestRepository)
    {
        _identityManager = identityManager;
        _guestRepository = guestRepository;
    }

    public async Task<LoginOutputDto> Login(LoginUserDto model)
    {
        var user = await _identityManager.Login(model);
        if (user.Role == UserRoles.Admin.ToString())
        {
            return new LoginOutputDto(user.UserId.ToString(), user.Token);
        }

        var guest = await _guestRepository.GetGuestByUserIdAsync(user.UserId)
            ?? throw new NotFoundException(nameof(Guest), user.UserId);
        var outputModel = new LoginOutputDto(guest.Id.ToString(), user.Token);

        return outputModel;
    }

    public async Task<IUser> RegisterAdmin(RegisterAdminDto model)
    {
        var user = await _identityManager.RegisterUser(model, UserRoles.Admin.ToString());

        return user;
    }

    public async Task<IUser> RegisterGuest(RegisterGuestDto model)
    {
        var user = await _identityManager.RegisterUser(model, UserRoles.Guest.ToString());
        var guest = new Guest(model.FirstName, model.LastName, model.DateOfBirth, model.Address);
        var guestEntity = await _guestRepository.AddGuestAsync(guest);
        user.RegisterUserAsGuest(guestEntity);
        await _guestRepository.SaveChangesAsync();

        return user;
    }
}
