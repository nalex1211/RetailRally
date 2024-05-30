using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class User : IdentityUser
{
    [Required(ErrorMessage = "Необхідне ім'я!")]
    public string? FirstName { get; set; }
    [Required(ErrorMessage = "Необхідне прізвище!")]
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PictureUrl { get; set; }
    public Cart? Cart { get; set; }
    public List<Product>? Products { get; set; } = new List<Product>();
    public List<Comment>? Comments { get; set; } = new List<Comment>();
}
