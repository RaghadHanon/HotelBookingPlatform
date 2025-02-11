using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Identity;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<LoginOutputDto> Login(LoginUserDto model);
    Task<IUser> RegisterUser(RegisterUserDto model, UserRoles role);
}
