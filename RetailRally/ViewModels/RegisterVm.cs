using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class RegisterVm
{
    [Required(ErrorMessage = "Ім'я обов'язкове!")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Прізвище обов'язкове!")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Обов'язкова наявність електронної пошти!")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Номер телефону обов'язковий!")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Ім'я користувача обов'язкове!")]
    public string Username { get; set; }
    [Required(ErrorMessage = "Потрібен пароль!")]

    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Потрібне підтвердження пароля!")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Паролі не збігаються!")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Дата народження обов'язкова!")]
    public DateTime BirthDate { get; set; } = DateTime.Now;
}
