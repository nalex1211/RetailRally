using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class NewPasswordVm
{
    public string UserId { get; set; }
    [Required(ErrorMessage = "New password is required!")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirmation is required!")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords don't match!")]
    public string ConfirmPassword { get; set; }
}
