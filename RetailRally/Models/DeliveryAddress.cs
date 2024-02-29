namespace RetailRally.Models;

public class DeliveryAddress
{
    public int Id { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}
