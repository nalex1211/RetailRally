using RetailRally.Models;
using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class AllUserVm
{
    public string Id { get; set; }
    [Required(ErrorMessage = "Необхідно вказати своє ім'я")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я повинно містити від 3 до 100 символів.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Необхідно вказати своє прізвище")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Прізвище повинно містити від 3 до 100 символів.")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Необхідно вказати електронну адресу")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Електронна адеса має містити від 3 до 50 символів.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Необхідно вказати ім'я користувача")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача має містити від 3 до 50 символів.")]
    public string UserName { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
