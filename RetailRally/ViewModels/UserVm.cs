using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class UserVm
{
    [Required(ErrorMessage = "Ім'я є обов'язковим для заповнення.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Прізвище є обов'язковим для заповнення.")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Номер телефону є обов'язковим для заповнення.")]
    public string PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PictureUrl { get; set; }
    [Required(ErrorMessage = "Ім'я користувача є обов'язковим для заповнення.")]
    public string UserName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool IsEditFormOpen { get; set; } = false;
}
