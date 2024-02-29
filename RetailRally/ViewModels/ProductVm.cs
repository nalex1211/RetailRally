using RetailRally.Models;

namespace RetailRally.ViewModels;

public class ProductVm
{
    public Product Product { get; set; }
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public User User { get; set; }
    public Comment NewComment { get; set; } = new Comment();
}
