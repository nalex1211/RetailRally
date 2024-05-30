using RetailRally.Models;

namespace RetailRally.ViewModels;

public class BuyProductVm
{
    public List<Product>? Products { get; set; } = new List<Product>();
    public User? User { get; set; }
    public Order Order { get; set; } = new Order();
    public DeliveryAddress? DeliveryAddress { get; set; }
    public Dictionary<int, int> ProductQuantities { get; set; } = new Dictionary<int, int>();
}
