using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewsModels;

public class LoginViewModel
{
    [Required] 
    [EmailAddress] 
    public string Email { get; set; } = null!;
    
    [Required]
    [DataType(dataType: DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}