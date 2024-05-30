using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class NewPasswordVm
{
    public string UserId { get; set; }
    [Required(ErrorMessage = "Потрібен новий пароль!")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Потрібне підтвердження пароля!")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Паролі не збігаються!")]
    public string ConfirmPassword { get; set; }
}
