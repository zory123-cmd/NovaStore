using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.WebApi.Controllers;

namespace NovaStore.WebApi.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IValidator<CreateCategoryDto>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateCategoryDto>> _updateValidatorMock;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _createValidatorMock = new Mock<IValidator<CreateCategoryDto>>();
            _updateValidatorMock = new Mock<IValidator<UpdateCategoryDto>>();
            _controller = new CategoriesController(
                _categoryServiceMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            var categories = new List<CategoryDto>
            {
                new CategoryDto { Id = 1, Name = "Electronics" }
            };
            _categoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<CategoryDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOkResult()
        {
            var category = new CategoryDto { Id = 1, Name = "Electronics" };
            _categoryServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(category);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal("Electronics", returnValue.Name);
        }

        [Fact]
        public async Task GetById_NonExisting_ReturnsNotFound()
        {
            _categoryServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((CategoryDto?)null);

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_ValidData_ReturnsCreatedResult()
        {
            var dto = new CreateCategoryDto { Name = "New Category" };
            var created = new CategoryDto { Id = 1, Name = "New Category" };

            _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _categoryServiceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

            var result = await _controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            _categoryServiceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_CategoryWithProducts_ReturnsBadRequest()
        {
            _categoryServiceMock.Setup(s => s.DeleteAsync(1))
                .ThrowsAsync(new InvalidOperationException("Cannot delete category with associated products."));

            var result = await _controller.Delete(1);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
