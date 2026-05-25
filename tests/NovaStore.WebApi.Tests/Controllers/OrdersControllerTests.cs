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
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IValidator<CreateOrderDto>> _createOrderValidatorMock;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _createOrderValidatorMock = new Mock<IValidator<CreateOrderDto>>();
            _controller = new OrdersController(_orderServiceMock.Object, _createOrderValidatorMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetMyOrders_ReturnsOkResult()
        {
            var orders = new PagedResult<OrderDto>
            {
                Items = new List<OrderDto> { new OrderDto { Id = 1, TotalAmount = 100 } },
                TotalCount = 1,
                Page = 1,
                PageSize = 20
            };
            _orderServiceMock.Setup(s => s.GetUserOrdersAsync(1, 1, 20)).ReturnsAsync(orders);

            var result = await _controller.GetMyOrders();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResult<OrderDto>>(okResult.Value);
            Assert.Single(returnValue.Items);
        }

        [Fact]
        public async Task GetById_ExistingOrder_ReturnsOkResult()
        {
            var order = new OrderDto { Id = 1, UserId = 1, TotalAmount = 100 };
            _orderServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(order);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task GetById_NonExistingOrder_ReturnsNotFound()
        {
            _orderServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((OrderDto?)null);

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateFromCart_ValidData_ReturnsCreatedResult()
        {
            var dto = new CreateOrderDto { PaymentMethod = "Card" };
            var created = new OrderDto { Id = 1, TotalAmount = 100 };

            _createOrderValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _orderServiceMock.Setup(s => s.CreateFromCartAsync(1, dto)).ReturnsAsync(created);

            var result = await _controller.CreateFromCart(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateFromCart_EmptyCart_ReturnsBadRequest()
        {
            var dto = new CreateOrderDto { PaymentMethod = "Card" };

            _createOrderValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _orderServiceMock.Setup(s => s.CreateFromCartAsync(1, dto))
                .ThrowsAsync(new InvalidOperationException("Cart is empty."));

            var result = await _controller.CreateFromCart(dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsAdmin_ReturnsOkResult()
        {
            var orders = new PagedResult<OrderDto>
            {
                Items = new List<OrderDto> { new OrderDto { Id = 1 } },
                TotalCount = 1,
                Page = 1,
                PageSize = 20
            };
            _orderServiceMock.Setup(s => s.GetAllAsync(1, 20)).ReturnsAsync(orders);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResult<OrderDto>>(okResult.Value);
            Assert.Single(returnValue.Items);
        }

        [Fact]
        public async Task CancelOrder_ValidOrder_ReturnsOkResult()
        {
            var cancelled = new OrderDto { Id = 1, Status = "Cancelled" };
            _orderServiceMock.Setup(s => s.CancelOrderAsync(1, 1)).ReturnsAsync(cancelled);

            var result = await _controller.CancelOrder(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal("Cancelled", returnValue.Status);
        }

        [Fact]
        public async Task CancelOrder_NonExistingOrder_ReturnsNotFound()
        {
            _orderServiceMock.Setup(s => s.CancelOrderAsync(999, 1))
                .ThrowsAsync(new KeyNotFoundException("Order with ID 999 not found."));

            var result = await _controller.CancelOrder(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CancelOrder_InvalidStatus_ReturnsBadRequest()
        {
            _orderServiceMock.Setup(s => s.CancelOrderAsync(1, 1))
                .ThrowsAsync(new InvalidOperationException("Cannot cancel order in 'Delivered' status."));

            var result = await _controller.CancelOrder(1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}
