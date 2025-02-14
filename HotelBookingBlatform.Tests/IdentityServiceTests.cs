using AutoFixture;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Identity;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Moq;
namespace HotelBookingBlatform.Application.Tests;

public class IdentityServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IIdentityManager> _identityManagerMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly AuthenticationService sut;

    public IdentityServiceTests()
    {
        _fixture = FixtureFactory.CreateFixture();
        _identityManagerMock = new Mock<IIdentityManager>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        sut = new AuthenticationService(_identityManagerMock.Object, _guestRepositoryMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnTokenForAdmin_WhenRoleIsAdminAndLoginSuccess()
    {
        // Arrange
        LoginUserDto loginUserModel = new LoginUserDto("Admin@example.com", "StrongPass12$");
        string userId = "ghsdk-lkjhs-557sl";
        string validToken = "khgrhgohrsoighrs.dajkgshgurhgiuvbrsivb.lsnghrsuhgsurhgur";
        LoginSuccessDto loginSuccessModel = new LoginSuccessDto(userId, validToken, UserRoles.Admin.ToString());
        _identityManagerMock.Setup(x => x.Login(loginUserModel)).ReturnsAsync(loginSuccessModel);

        // Act
        LoginOutputDto result = await sut.Login(loginUserModel);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LoginOutputDto>(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(validToken, result.Token);
    }

    [Fact]
    public async Task Login_ShouldReturnTokenForGuest_WhenRoleIsGuestAndLoginSuccess()
    {
        // Arrange
        LoginUserDto loginUserModel = new LoginUserDto("Guest@example.com", "StrongPass12$");
        string userId = "oioyut-lkjhs-557sl";
        string validToken = "khgrhgohrsoighrs.jhfhtdejjggffrsrsio.yrdreagouphifpojgut";
        LoginSuccessDto loginSuccessModel = new LoginSuccessDto(userId, validToken, UserRoles.Guest.ToString());
        Guest guest = _fixture.Build<Guest>()
                    .Without(g => g.Bookings)
                    .Create();
        _identityManagerMock.Setup(x => x.Login(loginUserModel)).ReturnsAsync(loginSuccessModel);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync(guest);

        // Act
        LoginOutputDto result = await sut.Login(loginUserModel);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LoginOutputDto>(result);
        Assert.Equal(guest.Id.ToString(), result.UserId);
        Assert.Equal(validToken, result.Token);
    }

    [Fact]
    public async Task Login_ShouldThrowNotFoundException_WhenGuestNotFound()
    {
        // Arrange
        LoginUserDto loginUserModel = new LoginUserDto("guest@example.com", "StrongPass12$");
        string userId = "oioyut-lkjhs-557sl";
        string validToken = "khgrhgohrsoighrs.jhfhtdejjggffrsrsio.yrdreagouphifpojgut";
        LoginSuccessDto loginSuccessModel = new LoginSuccessDto(userId, validToken, UserRoles.Guest.ToString());
        _identityManagerMock.Setup(x => x.Login(loginUserModel)).ReturnsAsync(loginSuccessModel);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Guest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.Login(loginUserModel));
    }

    [Fact]
    public async Task RegisterUser_ShouldRegisterAdmin_WhenRoleIsAdmin()
    {
        // Arrange
        RegisterUserDto registerUserModel = _fixture.Create<RegisterUserDto>();
        Mock<IUser> userMock = new Mock<IUser>();
        _identityManagerMock.Setup(x => x.RegisterUser(registerUserModel, UserRoles.Admin.ToString()))
                           .ReturnsAsync(userMock.Object);

        // Act
        IUser result = await sut.RegisterUser(registerUserModel, UserRoles.Admin);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IUser>(result);
        _identityManagerMock.Verify(x => x.RegisterUser(registerUserModel, UserRoles.Admin.ToString()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_ShouldRegisterGuest_WhenRoleIsGuest()
    {
        // Arrange
        RegisterUserDto registerUserDto = _fixture.Create<RegisterUserDto>();
        Mock<IUser> userMock = new Mock<IUser>();
        Guest guest = new Guest(registerUserDto.FirstName, registerUserDto.LastName, registerUserDto.DateOfBirth, registerUserDto.Address);
        _identityManagerMock.Setup(x => x.RegisterUser(registerUserDto, UserRoles.Guest.ToString()))
                           .ReturnsAsync(userMock.Object);
        _guestRepositoryMock.Setup(x => x.AddGuestAsync(It.IsAny<Guest>())).ReturnsAsync(guest);

        // Act
        IUser result = await sut.RegisterUser(registerUserDto, UserRoles.Guest);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IUser>(result);
        userMock.Verify(x => x.RegisterUserAsGuest(It.IsAny<Guest>()), Times.Once);
        _guestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _guestRepositoryMock.Verify(x => x.AddGuestAsync(It.IsAny<Guest>()), Times.Once);
    }
}
