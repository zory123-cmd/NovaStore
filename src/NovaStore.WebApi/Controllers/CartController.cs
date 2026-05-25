using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;

namespace NovaStore.WebApi.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly IValidator<AddToCartDto> _addToCartValidator;
        private readonly IValidator<UpdateCartItemDto> _updateCartItemValidator;

        public CartController(
            ICartService cartService,
            IValidator<AddToCartDto> addToCartValidator,
            IValidator<UpdateCartItemDto> updateCartItemValidator)
        {
            _cartService = cartService;
            _addToCartValidator = addToCartValidator;
            _updateCartItemValidator = updateCartItemValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var items = await _cartService.GetUserCartAsync(GetUserId());
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var validationResult = await _addToCartValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var item = await _cartService.AddToCartAsync(GetUserId(), dto);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateCartItemDto dto)
        {
            var validationResult = await _updateCartItemValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var result = await _cartService.UpdateQuantityAsync(GetUserId(), id, dto);
                if (!result)
                    return NotFound(new { message = $"Cart item with ID {id} not found." });
                return Ok(new { message = "Cart updated." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var result = await _cartService.RemoveFromCartAsync(GetUserId(), id);
            if (!result)
                return NotFound(new { message = $"Cart item with ID {id} not found." });
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return NoContent();
        }
    }
}
