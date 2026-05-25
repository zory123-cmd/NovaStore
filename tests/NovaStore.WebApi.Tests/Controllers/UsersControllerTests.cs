using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.WebApi.Controllers;

namespace NovaStore.WebApi.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IValidator<ChangePasswordDto>> _changePasswordValidatorMock;
        private readonly Mock<IValidator<CreateAddressDto>> _createAddressValidatorMock;
        private readonly Mock<IValidator<UpdateUserDto>> _updateUserValidatorMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _changePasswordValidatorMock = new Mock<IValidator<ChangePasswordDto>>();
            _createAddressValidatorMock = new Mock<IValidator<CreateAddressDto>>();
            _updateUserValidatorMock = new Mock<IValidator<UpdateUserDto>>();
            _controller = new UsersController(
                _userServiceMock.Object,
                _changePasswordValidatorMock.Object,
                _createAddressValidatorMock.Object,
                _updateUserValidatorMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetProfile_ReturnsOkResult()
        {
            var user = new UserDto { Id = 1, Username = "testuser", Email = "test@test.com" };
            _userServiceMock.Setup(s => s.GetProfileAsync(1)).ReturnsAsync(user);

            var result = await _controller.GetProfile();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("testuser", returnValue.Username);
        }

        [Fact]
        public async Task UpdateProfile_ValidData_ReturnsOkResult()
        {
            var dto = new UpdateUserDto { FullName = "New Name" };
            var updated = new UserDto { Id = 1, FullName = "New Name" };

            _updateUserValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _userServiceMock.Setup(s => s.UpdateProfileAsync(1, dto)).ReturnsAsync(updated);

            var result = await _controller.UpdateProfile(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("New Name", returnValue.FullName);
        }

        [Fact]
        public async Task ChangePassword_ValidData_ReturnsOkResult()
        {
            var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "NewPass123!" };

            _changePasswordValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _userServiceMock.Setup(s => s.ChangePasswordAsync(1, dto)).ReturnsAsync(true);

            var result = await _controller.ChangePassword(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAddresses_ReturnsOkResult()
        {
            var addresses = new List<AddressDto>
            {
                new AddressDto { Id = 1, City = "Moscow" }
            };
            _userServiceMock.Setup(s => s.GetAddressesAsync(1)).ReturnsAsync(addresses);

            var result = await _controller.GetAddresses();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<AddressDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task AddAddress_ValidData_ReturnsCreatedResult()
        {
            var dto = new CreateAddressDto { FullName = "Test", Phone = "+7", Street = "Lenina", City = "Moscow", ZipCode = "123456" };
            var created = new AddressDto { Id = 1, City = "Moscow" };

            _createAddressValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _userServiceMock.Setup(s => s.AddAddressAsync(1, dto)).ReturnsAsync(created);

            var result = await _controller.AddAddress(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_Existing_ReturnsNoContent()
        {
            _userServiceMock.Setup(s => s.DeleteAddressAsync(1, 1)).ReturnsAsync(true);

            var result = await _controller.DeleteAddress(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
