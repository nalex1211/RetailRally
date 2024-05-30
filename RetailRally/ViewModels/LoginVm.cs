using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class LoginVm
{
    [Required(ErrorMessage = "Обов'язкова наявність електронної пошти!")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Необхідно ввести пароль!")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
