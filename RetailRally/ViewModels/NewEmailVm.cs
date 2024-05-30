using System.ComponentModel.DataAnnotations;

namespace RetailRally.ViewModels;

public class NewEmailVm
{
    public string UserId { get; set; }
    public string OldEmail { get; set; }
    [Required(ErrorMessage = "Потрібна нова електронна адреса!")]
    [EmailAddress]
    public string NewEmail { get; set; }
}
