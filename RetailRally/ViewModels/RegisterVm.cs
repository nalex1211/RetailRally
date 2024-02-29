using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class RegisterVm
{
    [Required(ErrorMessage ="First name is required!")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required!")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required!")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required!")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Username is required!")]
    public string Username { get; set; }

    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirmation is required!")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords don't match!")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Birthdate is required!")]
    public DateTime BirthDate { get; set; } = DateTime.Now;
}
