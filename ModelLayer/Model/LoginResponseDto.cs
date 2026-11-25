namespace UserService.Application.Models;

public class LoginResponseDto
{
    public UserResponseDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
