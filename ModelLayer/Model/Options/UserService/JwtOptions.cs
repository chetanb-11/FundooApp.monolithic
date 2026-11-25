using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Models.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Issuer { get; set; } = default!;
    [Required, MinLength(32)] public string SigningKey { get; init; } = default!;
    [Required, MinLength(32)] public string ResetSigningKey { get; init; } = default!;
    public int AccessTokenMinutes { get; init; } = 10;
}
