using MailKit.Search;

namespace RetailRally.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
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
