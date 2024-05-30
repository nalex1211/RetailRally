using MailKit.Search;
using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class Product
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Необхідно вказати назву продукту!")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Назва продукту повинна містити від 3 до 100 символів.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Опис товару обов'язковий!")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Опис товару повинен містити від 10 до 500 символів.")]
    public string Description { get; set; }
    [Required(ErrorMessage = "Необхідно вказати ціну товару!")]
    [Range(1, 1e8, ErrorMessage = "Ціна повинна бути від 1 до 100,000,000")]
    public float Price { get; set; }
    public string UserId { get; set; }
    public User? User { get; set; }
    public string? PictureUrl { get; set; }
    public int? CategoryId { get; set; }
    [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100,000")]
    public int Quantity { get; set; }
    public Category? Category { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<Comment>? Comments { get; set; } = new List<Comment>();
    public List<OrderProduct>? OrderProducts { get; set; } = new List<OrderProduct>();
}
