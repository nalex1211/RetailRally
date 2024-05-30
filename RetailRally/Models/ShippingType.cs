using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class ShippingType
{
    public int Id {  get; set; }
    [Required(ErrorMessage = "Введіть назву типу доставки!")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Назва типу доставки повина містити від 2 до 150 символів.")]
    public string Name { get; set; }
}
