using System.ComponentModel.DataAnnotations;

namespace NoteService.Application.Models.Options;

/// <summary>
/// NoteService only VALIDATES tokens issued by UserService, so it needs the same
/// Issuer + SigningKey, but not ResetSigningKey / AccessTokenMinutes.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Issuer { get; set; } = default!;
    [Required, MinLength(32)] public string SigningKey { get; init; } = default!;
}
