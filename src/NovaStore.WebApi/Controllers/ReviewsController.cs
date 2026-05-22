using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;

namespace NovaStore.WebApi.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IValidator<CreateReviewDto> _createReviewValidator;

        public ReviewsController(IReviewService reviewService, IValidator<CreateReviewDto> createReviewValidator)
        {
            _reviewService = reviewService;
            _createReviewValidator = createReviewValidator;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token.");
            return userId;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _reviewService.GetByProductAsync(productId, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            var validationResult = await _createReviewValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var userId = GetUserId();

            try
            {
                var review = await _reviewService.CreateAsync(userId, dto);
                return CreatedAtAction(nameof(GetByProduct), new { productId = dto.ProductId }, review);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateReviewDto dto)
        {
            var validationResult = await _createReviewValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var userId = GetUserId();

            var review = await _reviewService.UpdateAsync(userId, id, dto);
            if (review == null)
                return NotFound(new { message = $"Review with ID {id} not found." });
            return Ok(review);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var result = await _reviewService.DeleteAsync(userId, id);
            if (!result)
                return NotFound(new { message = $"Review with ID {id} not found." });
            return NoContent();
        }
    }
}
