using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;

namespace NovaStore.WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IValidator<CreateAddressDto> _createAddressValidator;
        private readonly IValidator<UpdateUserDto> _updateUserValidator;

        public UsersController(
            IUserService userService,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<CreateAddressDto> createAddressValidator,
            IValidator<UpdateUserDto> updateUserValidator)
        {
            _userService = userService;
            _changePasswordValidator = changePasswordValidator;
            _createAddressValidator = createAddressValidator;
            _updateUserValidator = updateUserValidator;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token.");
            return userId;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userService.GetProfileAsync(GetUserId());
            if (user == null)
                return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var validationResult = await _updateUserValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var user = await _userService.UpdateProfileAsync(GetUserId(), dto);
            if (user == null)
                return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var validationResult = await _changePasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var result = await _userService.ChangePasswordAsync(GetUserId(), dto);
                if (!result)
                    return NotFound(new { message = "User not found." });
                return Ok(new { message = "Password changed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var addresses = await _userService.GetAddressesAsync(GetUserId());
            return Ok(addresses);
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
        {
            var validationResult = await _createAddressValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var address = await _userService.AddAddressAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetAddresses), null, address);
        }

        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] CreateAddressDto dto)
        {
            var validationResult = await _createAddressValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var address = await _userService.UpdateAddressAsync(GetUserId(), id, dto);
            if (address == null)
                return NotFound(new { message = $"Address with ID {id} not found." });
            return Ok(address);
        }

        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var result = await _userService.DeleteAddressAsync(GetUserId(), id);
            if (!result)
                return NotFound(new { message = $"Address with ID {id} not found." });
            return NoContent();
        }
    }
}
