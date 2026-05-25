using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;

namespace NovaStore.WebApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IValidator<CreateOrderDto> _createOrderValidator;

        public OrdersController(IOrderService orderService, IValidator<CreateOrderDto> createOrderValidator)
        {
            _orderService = orderService;
            _createOrderValidator = createOrderValidator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery][Range(1, 10000)] int page = 1, [FromQuery][Range(1, 100)] int pageSize = 20)
        {
            var orders = await _orderService.GetAllAsync(page, pageSize);
            return Ok(orders);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders([FromQuery][Range(1, 10000)] int page = 1, [FromQuery][Range(1, 100)] int pageSize = 20)
        {
            var orders = await _orderService.GetUserOrdersAsync(GetUserId(), page, pageSize);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found." });

            // Customers can only see their own orders
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && order.UserId != userId)
                return Forbid();

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromCart([FromBody] CreateOrderDto dto)
        {
            var validationResult = await _createOrderValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var order = await _orderService.CreateFromCartAsync(GetUserId(), dto);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
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

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderDto dto)
        {
            var order = await _orderService.UpdateStatusAsync(id, dto);
            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found." });
            return Ok(order);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var order = await _orderService.CancelOrderAsync(id, GetUserId());
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Order with ID {id} not found." });
            return NoContent();
        }
    }
}
