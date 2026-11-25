using System.ComponentModel.DataAnnotations;

namespace NoteService.Domain.Entities;

public class NoteEntity
{
    [Key] public int Id { get; set; }

    // Owner of the note. This is the user id from UserService (JWT "sub" claim).
    // No foreign key / navigation property: users live in a different database now.
    public int UserId { get; set; }

    [Required, MaxLength(50)] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsTrashed { get; set; }
}
