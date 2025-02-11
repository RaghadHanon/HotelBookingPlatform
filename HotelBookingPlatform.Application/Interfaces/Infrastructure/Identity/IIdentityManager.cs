using HotelBookingPlatform.Application.DTOs.Identity;

namespace HotelBookingPlatform.Application.Interfaces.Identity;

public interface IIdentityManager
{
    Task<LoginSuccessDto> Login(LoginUserDto model);
    Task<IUser> RegisterUser(RegisterUserDto model, string role);
}
