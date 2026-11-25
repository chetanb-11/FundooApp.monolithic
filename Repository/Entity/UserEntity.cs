using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.Entities;

public class UserEntity
{
    [Required] [Key] public int Id { get; set; }


    [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(20)] public string Role { get; set; } = "User";

    [Required, RegularExpression("^[0-9]{10}$")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required] public byte[]? Password { get; set; }
    [Required] public byte[]? Salt { get; set; }

    // NOTE: the Notes navigation collection was removed — notes now live in NoteService's own database.
}
