using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class UserVm
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PictureUrl { get; set; }
    [Required]
    public string UserName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool IsEditFormOpen { get; set; } = false;
}
