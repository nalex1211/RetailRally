using MailKit.Search;
using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class Product
{
    public int Id { get; set; }
    [Required(ErrorMessage ="Name of product is required!")]
    public string Name { get; set; }
    [Required(ErrorMessage ="Description of product is required!")]
    public string Description { get; set; }
    [Required(ErrorMessage ="Price of product is required!")]
    public float Price { get; set; }
    public string UserId { get; set; }
    public User? User { get; set; }
    public string? PictureUrl { get; set; }
    public uint Quantity { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<Comment>? Comments { get; set; } = new List<Comment>();
    public List<OrderProduct>? OrderProducts { get; set; } = new List<OrderProduct>();
}
