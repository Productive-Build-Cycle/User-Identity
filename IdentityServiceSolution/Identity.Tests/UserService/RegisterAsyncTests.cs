using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Roles;
using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Identity.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.Tests.UserService;

public class RegisterAsyncTests
{
    // Mocks
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IRolesService> _rolesServiceMock;

    // System Under Test
    private readonly UserrService _sut;

    public RegisterAsyncTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null, null, null, null, null, null, null, null
        );

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>()
        );

        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();
        _mapperMock = new Mock<IMapper>();
        _emailServiceMock = new Mock<IEmailService>();
        _rolesServiceMock = new Mock<IRolesService>();

        _configurationMock.Setup(c => c["BaseUrl"]).Returns("https://example.com");

        _sut = new UserrService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenServiceMock.Object,
            _configurationMock.Object,
            _mapperMock.Object,
            _emailServiceMock.Object,
            _rolesServiceMock.Object
        );
    }

    //[Fact]
    //public async Task RegisterAsync_WhenEmailIsNew_ShouldRegisterUserAndSendEmail()
    //{
    //    // Arrange
    //    var request = new RegisterRequest
    //    {
    //        Email = "test@example.com",
    //        Password = "StrongPassword123!",
    //        FirstName = "Test",
    //        LastName = "User"
    //    };

    //    _userManagerMock
    //        .Setup(x => x.FindByEmailAsync(request.Email))
    //        .ReturnsAsync((ApplicationUser?)null);

    //    var user = new ApplicationUser
    //    {
    //        Id = Guid.NewGuid(),
    //        Email = request.Email,
    //        UserName = request.Email
    //    };

    //    _mapperMock
    //        .Setup(x => x.Map<ApplicationUser>(request))
    //        .Returns(user);

    //    _userManagerMock
    //        .Setup(x => x.CreateAsync(user, request.Password))
    //        .ReturnsAsync(IdentityResult.Success);

    //    _rolesServiceMock
    //        .Setup(x => x.AddUserToRoleAsync(It.IsAny<AssignRoleToUserRequest>()))
    //        .Returns(Task.CompletedTask);

    //    _userManagerMock
    //        .Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
    //        .ReturnsAsync("CONFIRM_TOKEN");

    //    _emailServiceMock
    //        .Setup(x => x.TurnHtmlToString("EmailConfirmation.html", It.IsAny<Dictionary<string, string>>()))
    //        .ReturnsAsync("EMAIL_BODY");

    //    _emailServiceMock
    //        .Setup(x => x.SendEmailAsync(It.IsAny<EmailOptions>()))
    //        .Returns(Task.CompletedTask);

    //    // Act
    //    var result = await _sut.RegisterAsync(request);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.False(result.IsEmailConfirmed);
    //    Assert.Equal(request.Email, result.Email);

    //    _userManagerMock.Verify(x => x.CreateAsync(user, request.Password), Times.Once);
    //    _rolesServiceMock.Verify(x => x.AddUserToRoleAsync(It.IsAny<AssignRoleToUserRequest>()), Times.Once);
    //    _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<EmailOptions>()), Times.Once);
    //}

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowDuplicateEmailError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "StrongPassword123!"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser { Email = request.Email });

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains(request.Email, ex.Message);

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _rolesServiceMock.Verify(x => x.AddUserToRoleAsync(It.IsAny<AssignRoleToUserRequest>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<EmailOptions>()), Times.Never);
    }
}
