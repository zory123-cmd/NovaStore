using NovaStore.Application.DTOs;

namespace NovaStore.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetProfileAsync(int userId);
        Task<UserDto?> UpdateProfileAsync(int userId, UpdateUserDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<List<AddressDto>> GetAddressesAsync(int userId);
        Task<AddressDto> AddAddressAsync(int userId, CreateAddressDto dto);
        Task<AddressDto?> UpdateAddressAsync(int userId, int addressId, CreateAddressDto dto);
        Task<bool> DeleteAddressAsync(int userId, int addressId);
    }
}
