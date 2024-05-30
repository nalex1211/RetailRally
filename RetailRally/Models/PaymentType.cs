using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class PaymentType
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введіть назву типу оплати!")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Назва типу оплати повина містити від 2 до 150 символів.")]
    public string Name { get; set; }
}
