using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class DeliveryAddress
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введіть країну доставки!")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Країна доставки повина містити від 2 до 200 символів.")]
    public string? Country { get; set; }
    [Required(ErrorMessage = "Введіть місто доставки!")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Місто доставки повина містити від 2 до 200 символів.")]
    public string? City { get; set; }
    [Required(ErrorMessage = "Введіть вулицю доставки!")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Вулиця доставки повина містити від 2 до 200 символів.")]
    public string? Street { get; set; }
    [Required(ErrorMessage = "Введіть поштовий індекс доставки!")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Поштовий індекс доставки повинен містити від 2 до 30 символів.")]
    public string? PostalCode { get; set; }
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}
