using HotelBookingPlatform.Application.DTOs.Identity;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<LoginOutputDto> Login(LoginUserDto model);
    Task<IUser> RegisterAdmin(RegisterAdminDto model);
    Task<IUser> RegisterGuest(RegisterGuestDto model);
}
