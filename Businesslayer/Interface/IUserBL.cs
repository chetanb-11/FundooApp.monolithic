using UserService.Application.Models;

namespace UserService.Application.Interfaces;

public interface IUserBl
{
    Task<UserResponseDto?> RegisterUser(UserRegistrationReq userReq);
    public Task<bool> UserExistsAsync(string email);
    public Task<bool> UserExistsByIdAsync(int userId);
    Task<LoginResponseDto?> LoginUser(UserLoginReq userReq);
    Task<bool> ResetPassword(string token, string newPassword);
    public Task<bool> ForgetPassword(string email);
}
