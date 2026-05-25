using Microsoft.EntityFrameworkCore;
using NovaStore.Application.DTOs;
using NovaStore.Application.Interfaces;
using NovaStore.Domain.Data;
using NovaStore.Domain.Models;

namespace NovaStore.Application.Services
{
    public class UserService : IUserService
    {
        private readonly NovaStoreDbContext _context;

        public UserService(NovaStoreDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : MapUserToDto(user);
        }

        public async Task<UserDto?> UpdateProfileAsync(int userId, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

            await _context.SaveChangesAsync();
            return MapUserToDto(user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AddressDto>> GetAddressesAsync(int userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return addresses.Select(MapAddressToDto).ToList();
        }

        public async Task<AddressDto> AddAddressAsync(int userId, CreateAddressDto dto)
        {
            if (dto.IsDefault)
            {
                var currentDefault = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
                if (currentDefault != null)
                    currentDefault.IsDefault = false;
            }

            var address = new Address
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                Country = dto.Country,
                IsDefault = dto.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return MapAddressToDto(address);
        }

        public async Task<AddressDto?> UpdateAddressAsync(int userId, int addressId, CreateAddressDto dto)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return null;

            if (dto.IsDefault && !address.IsDefault)
            {
                var currentDefault = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.Id != addressId);
                if (currentDefault != null)
                    currentDefault.IsDefault = false;
            }

            address.FullName = dto.FullName;
            address.Phone = dto.Phone;
            address.Street = dto.Street;
            address.City = dto.City;
            address.State = dto.State;
            address.ZipCode = dto.ZipCode;
            address.Country = dto.Country;
            address.IsDefault = dto.IsDefault;

            await _context.SaveChangesAsync();
            return MapAddressToDto(address);
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        private static UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        private static AddressDto MapAddressToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                FullName = address.FullName,
                Phone = address.Phone,
                Street = address.Street,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Country = address.Country,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt
            };
        }
    }
}
