using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.WebApi.Controllers;

namespace NovaStore.WebApi.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IValidator<CreateProductDto>> _createValidatorMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _createValidatorMock = new Mock<IValidator<CreateProductDto>>();
            _controller = new ProductsController(_productServiceMock.Object, _createValidatorMock.Object);
        }

        [Fact]
        public async Task Search_ReturnsOkResult()
        {
            // Arrange
            var searchDto = new ProductSearchDto { Page = 1, PageSize = 10 };
            var pagedResult = new PagedResult<ProductDto>
            {
                Items = new List<ProductDto>
                {
                    new ProductDto { Id = 1, Name = "Test Product", Price = 100 }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _productServiceMock.Setup(s => s.SearchProductsAsync(searchDto))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.Search(searchDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResult<ProductDto>>(okResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal("Test Product", returnValue.Items[0].Name);
        }

        [Fact]
        public async Task GetById_ExistingProduct_ReturnsOkResult()
        {
            // Arrange
            var product = new ProductDto { Id = 1, Name = "Test Product", Price = 100 };
            _productServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal("Test Product", returnValue.Name);
        }

        [Fact]
        public async Task GetById_NonExistingProduct_ReturnsNotFound()
        {
            // Arrange
            _productServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProductDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "New Product", Price = 100, CategoryId = 1 };
            var created = new ProductDto { Id = 1, Name = "New Product", Price = 100, CategoryId = 1 };

            _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _productServiceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var returnValue = Assert.IsType<ProductDto>(createdResult.Value);
            Assert.Equal("New Product", returnValue.Name);
        }

        [Fact]
        public async Task Create_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "", Price = 0, CategoryId = 0 };
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required")
            };

            _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ExistingProduct_ReturnsNoContent()
        {
            // Arrange
            _productServiceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingProduct_ReturnsNotFound()
        {
            // Arrange
            _productServiceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
