using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Controller for Authentication-related actions such as registration and login
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService identityService)
    {
        _authenticationService = identityService;

    }

    /// <summary>
    /// Register an admin
    /// </summary>
    /// <param name="request">The registration request object</param>
    /// <returns>A success message with no content</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/Authentication/RegisterAdmin
    ///     {
    ///       "email": "Sample",
    ///       "password": "Sample_Password",
    ///       "confirmPassword": "Sample_Password",
    ///       "username": "Sample_UserName"
    ///     }
    ///     
    /// </remarks>
    /// <response code="201">Admin registered successfully</response>
    /// <response code="400">If the request data is invalid</response>
    [Authorize(Policy = Policies.AdminOnly)]  //Only an admin can add other admins
    [HttpPost("RegisterAdmin")]
    public async Task<ActionResult> RegisterAdmin([FromBody] RegisterAdminDto request)
    {
        await _authenticationService.RegisterAdmin(request);

        return Ok(new { Status = Status.Success, Message = APIMessages.UserRegisteredSuccessfully });
    }

    /// <summary>
    /// Register a guest
    /// </summary>
    /// <param name="request">The registration request object</param>
    /// <returns>A success message with no content</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/Authentication/RegisterGuest
    ///     {
    ///       "email": "Sample",
    ///       "password": "Sample_Password",
    ///       "confirmPassword": "Sample_Password",
    ///       "username": "Sample_UserName",
    ///       "firstName": "Sample_firstName",
    ///       "lastName": "Sample_lastName",
    ///       "dateOfBirth": "2000-01-24",
    ///       "address": "Sample"
    ///     }
    ///     
    /// </remarks>
    /// <response code="201">Guest registered successfully</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("RegisterGuest")]
    public async Task<ActionResult> RegisterGuest([FromBody] RegisterGuestDto request)
    {
        await _authenticationService.RegisterGuest(request);

        return Ok(new { Status = Status.Success, Message = APIMessages.UserRegisteredSuccessfully });
    }

    /// <summary>
    /// Login a user
    /// </summary>
    /// <param name="request">The login request object</param>
    /// <returns>A user ID and a token</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/Authentication/login
    ///     {
    ///        "email": "sample@user.com",
    ///        "password": "sample_password"
    ///     }
    ///     
    /// </remarks>
    /// <response code="200">Login successful</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("login")]
    public async Task<ActionResult<LoginOutputDto>> Login([FromBody] LoginUserDto request)
    {
        var result = await _authenticationService.Login(request);

        return Ok(result);
    }
}
