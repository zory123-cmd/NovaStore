using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.WebApi.Controllers;

namespace NovaStore.WebApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IValidator<RegisterUserDto>> _registerValidatorMock;
        private readonly Mock<IValidator<LoginDto>> _loginValidatorMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _registerValidatorMock = new Mock<IValidator<RegisterUserDto>>();
            _loginValidatorMock = new Mock<IValidator<LoginDto>>();
            _controller = new AuthController(
                _authServiceMock.Object,
                _registerValidatorMock.Object,
                _loginValidatorMock.Object);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOkResult()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "testuser", Email = "test@test.com", Password = "Password123!" };
            var response = new LoginResponseDto
            {
                Token = "test-token",
                User = new UserDto { Id = 1, Username = "testuser", Email = "test@test.com" }
            };

            _registerValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _authServiceMock.Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.Equal("test-token", returnValue.Token);
            Assert.Equal("testuser", returnValue.User.Username);
        }

        [Fact]
        public async Task Register_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "", Email = "", Password = "" };
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Username", "Username is required"),
                new ValidationFailure("Email", "Email is required")
            };

            _registerValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_DuplicateUser_ReturnsConflict()
        {
            // Arrange
            var dto = new RegisterUserDto { Username = "existing", Email = "existing@test.com", Password = "Password123!" };

            _registerValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _authServiceMock.Setup(s => s.RegisterAsync(dto))
                .ThrowsAsync(new InvalidOperationException("Username already exists."));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "Password123!" };
            var response = new LoginResponseDto
            {
                Token = "test-token",
                User = new UserDto { Id = 1, Username = "testuser" }
            };

            _loginValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _authServiceMock.Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.Equal("test-token", returnValue.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginDto { Username = "testuser", Password = "wrong" };

            _loginValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _authServiceMock.Setup(s => s.LoginAsync(dto))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid username or password."));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }
    }
}
