using AutoFixture;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Identity;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
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
        var loginUserModel = new LoginUserDto("Admin@example.com", "StrongPass12$");
        var userId = "ghsdk-lkjhs-557sl";
        var validToken = "khgrhgohrsoighrs.dajkgshgurhgiuvbrsivb.lsnghrsuhgsurhgur";
        var loginSuccessModel = new LoginSuccessDto(userId, validToken, UserRoles.Admin.ToString());
        _identityManagerMock.Setup(x => x.Login(loginUserModel)).ReturnsAsync(loginSuccessModel);

        // Act
        var result = await sut.Login(loginUserModel);

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
        var loginUserModel = new LoginUserDto("Guest@example.com", "StrongPass12$");
        var userId = "oioyut-lkjhs-557sl";
        var validToken = "khgrhgohrsoighrs.jhfhtdejjggffrsrsio.yrdreagouphifpojgut";
        var loginSuccessModel = new LoginSuccessDto(userId, validToken, UserRoles.Guest.ToString());
        var guest = _fixture.Build<Guest>()
                    .Without(g => g.Bookings)
                    .Create();
        _identityManagerMock.Setup(x => x.Login(loginUserModel)).ReturnsAsync(loginSuccessModel);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync(guest);

        // Act
        var result = await sut.Login(loginUserModel);

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
        var loginUserModel = new LoginUserDto("guest@example.com", "StrongPass12$");
        var userId = "oioyut-lkjhs-557sl";
        var validToken = "khgrhgohrsoighrs.jhfhtdejjggffrsrsio.yrdreagouphifpojgut";
        var loginSuccessModel = new LoginSuccessDto(userId, validToken, UserRoles.Guest.ToString());
        _identityManagerMock.Setup(x => x.Login(loginUserModel)).ReturnsAsync(loginSuccessModel);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Guest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.Login(loginUserModel));
    }

    [Fact]
    public async Task RegisterUser_ShouldRegisterAdmin_WhenRoleIsAdmin()
    {
        // Arrange
        var registerAdminModel = _fixture.Create<RegisterAdminDto>();
        var userMock = new Mock<IUser>();
        _identityManagerMock.Setup(x => x.RegisterUser(registerAdminModel, UserRoles.Admin.ToString()))
                           .ReturnsAsync(userMock.Object);

        // Act
        IUser result = await sut.RegisterAdmin(registerAdminModel);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IUser>(result);
        _identityManagerMock.Verify(x => x.RegisterUser(registerAdminModel, UserRoles.Admin.ToString()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_ShouldRegisterGuest_WhenRoleIsGuest()
    {
        // Arrange
        var registerGuestDto = _fixture.Create<RegisterGuestDto>();
        var userMock = new Mock<IUser>();
        var guest = new Guest(registerGuestDto.FirstName, registerGuestDto.LastName, registerGuestDto.DateOfBirth, registerGuestDto.Address);
        _identityManagerMock.Setup(x => x.RegisterUser(registerGuestDto, UserRoles.Guest.ToString()))
                           .ReturnsAsync(userMock.Object);
        _guestRepositoryMock.Setup(x => x.AddGuestAsync(It.IsAny<Guest>())).ReturnsAsync(guest);

        // Act
        IUser result = await sut.RegisterGuest(registerGuestDto);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IUser>(result);
        userMock.Verify(x => x.RegisterUserAsGuest(It.IsAny<Guest>()), Times.Once);
        _guestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _guestRepositoryMock.Verify(x => x.AddGuestAsync(It.IsAny<Guest>()), Times.Once);
    }
}
