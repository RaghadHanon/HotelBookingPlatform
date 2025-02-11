using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Identity;
using HotelBookingPlatform.Infrastructure.Authentication;
using HotelBookingPlatform.Infrastructure.Utilities;
using HotelBookingPlatform.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace HotelBookingPlatform.Infrastructure.Identity;

public class IdentityManager : IIdentityManager
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserValidator _userValidationService;

    public IdentityManager(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IUserValidator userValidationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userValidationService = userValidationService;
    }

    public async Task<LoginSuccessDto> Login(LoginUserDto model)
    {
        var user = await _userValidationService.ValidateUserCredentialsAsync(model);
        var roles = await FetchUserRolesAsync(user);
        var token = await GenerateJwtToken(user, roles);

        return new LoginSuccessDto(user.Id, token, roles.First());
    }

    private async Task<IList<string>> FetchUserRolesAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            throw new NoRolesException(user.Id);
        }

        return roles;
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        return await _jwtTokenGenerator.GenerateToken(user, roles)
            ?? throw new TokenGenerationFailedException(InfrastructureErrorMessages.NullToken);
    }

    public async Task<IUser> RegisterUser(RegisterUserDto model, string role)
    {
        _userValidationService.ValidatePasswordsMatch(model.Password, model.ConfirmPassword);
        var user = new ApplicationUser(model.Username, model.Email);
        await CreateUserAsync(user, model.Password);
        await AssignRoleToUserAsync(user, role);

        return user;
    }

    private async Task CreateUserAsync(ApplicationUser user, string password)
    {
        var identityResult = await _userManager.CreateAsync(user, password);
        if (!identityResult.Succeeded)
        {
            var errors = JsonSerializer.Serialize(identityResult.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }
    }

    private async Task AssignRoleToUserAsync(ApplicationUser user, string role)
    {
        await CreateRoleIfNotExistsAsync(role);
        await _userManager.AddToRoleAsync(user, role);
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
