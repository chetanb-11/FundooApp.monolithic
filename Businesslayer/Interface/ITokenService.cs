using UserService.Domain.Models;

namespace UserService.Application.Interfaces;

public interface ITokenService
{
    public string CreateAccessToken(UserDomainModel user);
    public string? CreateResetPasswordToken(string email);
    public Task<string?> ValidateTokenAndGetEmail(string token);
}
