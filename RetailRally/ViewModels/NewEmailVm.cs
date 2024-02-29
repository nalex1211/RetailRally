using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class NewEmailVm
{
    public string UserId { get; set; }
    public string OldEmail { get; set; }
    [Required(ErrorMessage = "New email is required!")]
    [EmailAddress]
    public string NewEmail { get; set; }
}
