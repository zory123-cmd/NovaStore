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
    public class ReviewsControllerTests
    {
        private readonly Mock<IReviewService> _reviewServiceMock;
        private readonly Mock<IValidator<CreateReviewDto>> _createReviewValidatorMock;
        private readonly ReviewsController _controller;

        public ReviewsControllerTests()
        {
            _reviewServiceMock = new Mock<IReviewService>();
            _createReviewValidatorMock = new Mock<IValidator<CreateReviewDto>>();
            _controller = new ReviewsController(_reviewServiceMock.Object, _createReviewValidatorMock.Object);

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
        public async Task GetByProduct_ReturnsOkResult()
        {
            var reviews = new PagedResult<ReviewDto>
            {
                Items = new List<ReviewDto> { new ReviewDto { Id = 1, Rating = 5 } },
                TotalCount = 1,
                Page = 1,
                PageSize = 20
            };
            _reviewServiceMock.Setup(s => s.GetByProductAsync(1, 1, 20)).ReturnsAsync(reviews);

            var result = await _controller.GetByProduct(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResult<ReviewDto>>(okResult.Value);
            Assert.Single(returnValue.Items);
        }

        [Fact]
        public async Task Create_ValidData_ReturnsCreatedResult()
        {
            var dto = new CreateReviewDto { ProductId = 1, Rating = 5 };
            var created = new ReviewDto { Id = 1, ProductId = 1, Rating = 5 };

            _createReviewValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _reviewServiceMock.Setup(s => s.CreateAsync(1, dto)).ReturnsAsync(created);

            var result = await _controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Create_DuplicateReview_ReturnsConflict()
        {
            var dto = new CreateReviewDto { ProductId = 1, Rating = 5 };

            _createReviewValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _reviewServiceMock.Setup(s => s.CreateAsync(1, dto))
                .ThrowsAsync(new InvalidOperationException("You have already reviewed this product."));

            var result = await _controller.Create(dto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Update_ExistingReview_ReturnsOkResult()
        {
            var dto = new CreateReviewDto { ProductId = 1, Rating = 4 };
            var updated = new ReviewDto { Id = 1, Rating = 4 };

            _createReviewValidatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _reviewServiceMock.Setup(s => s.UpdateAsync(1, 1, dto)).ReturnsAsync(updated);

            var result = await _controller.Update(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReviewDto>(okResult.Value);
            Assert.Equal(4, returnValue.Rating);
        }

        [Fact]
        public async Task Delete_ExistingReview_ReturnsNoContent()
        {
            _reviewServiceMock.Setup(s => s.DeleteAsync(1, 1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingReview_ReturnsNotFound()
        {
            _reviewServiceMock.Setup(s => s.DeleteAsync(1, 999)).ReturnsAsync(false);

            var result = await _controller.Delete(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
