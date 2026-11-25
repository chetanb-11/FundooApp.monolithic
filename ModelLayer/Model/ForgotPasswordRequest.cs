using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Models;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}