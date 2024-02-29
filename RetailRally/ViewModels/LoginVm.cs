using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class LoginVm
{
    [Required(ErrorMessage = "Email required!")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password required!")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
