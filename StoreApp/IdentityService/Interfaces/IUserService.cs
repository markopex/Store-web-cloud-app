using IdentityService.Dto;

namespace IdentityService.Interfaces
{
    public interface IUserService
    {
        UserDto GetUserByEmail(string email);
        UserDto GetUserByUsername(string username);
        UserDto AddUser(RegisterDto registerDto);
        UserDto UpdateUser(string email, UpdateUserDto dto);
        SuccessLoginDto LoginUser(LoginDto loginDto);
    }
}
