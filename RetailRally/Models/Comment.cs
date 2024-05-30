using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string UserId { get; set; }
    public User? User { get; set; }
    public double? Rating { get; set; }
}
