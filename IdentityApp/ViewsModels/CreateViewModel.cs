using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewsModels;

public class CreateViewModel
{
    [Required] 
    public string UserName { get; set; } = string.Empty;
    [Required] 
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(20)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}