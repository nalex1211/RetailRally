using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage ="Введіть назву категорії!")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Назва категорії повина містити від 2 до 150 символів.")]
    public string Name { get; set; }
    public List<Product>? Products { get; set; }
}
