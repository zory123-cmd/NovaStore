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
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IValidator<AddToCartDto>> _addToCartValidatorMock;
        private readonly Mock<IValidator<UpdateCartItemDto>> _updateCartItemValidatorMock;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _addToCartValidatorMock = new Mock<IValidator<AddToCartDto>>();
            _updateCartItemValidatorMock = new Mock<IValidator<UpdateCartItemDto>>();
            _controller = new CartController(
                _cartServiceMock.Object,
                _addToCartValidatorMock.Object,
                _updateCartItemValidatorMock.Object);

            // Setup user claims
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
        public async Task GetCart_ReturnsOkResult()
        {
            // Arrange
            var items = new List<CartItemDto>
            {
                new CartItemDto { Id = 1, ProductId = 1, Quantity = 2, UnitPrice = 100, Subtotal = 200 }
            };
            _cartServiceMock.Setup(s => s.GetUserCartAsync(1)).ReturnsAsync(items);

            // Act
            var result = await _controller.GetCart();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<CartItemDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task AddToCart_ValidData_ReturnsOkResult()
        {
            // Arrange
            var dto = new AddToCartDto { ProductId = 1, Quantity = 1 };
            var item = new CartItemDto { Id = 1, ProductId = 1, Quantity = 1, UnitPrice = 100 };

            _addToCartValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _cartServiceMock.Setup(s => s.AddToCartAsync(1, dto)).ReturnsAsync(item);

            // Act
            var result = await _controller.AddToCart(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CartItemDto>(okResult.Value);
            Assert.Equal(1, returnValue.ProductId);
        }

        [Fact]
        public async Task RemoveFromCart_ExistingItem_ReturnsNoContent()
        {
            // Arrange
            _cartServiceMock.Setup(s => s.RemoveFromCartAsync(1, 1)).ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveFromCart(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveFromCart_NonExistingItem_ReturnsNotFound()
        {
            // Arrange
            _cartServiceMock.Setup(s => s.RemoveFromCartAsync(1, 999)).ReturnsAsync(false);

            // Act
            var result = await _controller.RemoveFromCart(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ClearCart_ReturnsNoContent()
        {
            // Act
            var result = await _controller.ClearCart();

            // Assert
            Assert.IsType<NoContentResult>(result);
            _cartServiceMock.Verify(s => s.ClearCartAsync(1), Times.Once);
        }
    }
}
