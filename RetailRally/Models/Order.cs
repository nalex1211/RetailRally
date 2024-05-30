using System.ComponentModel.DataAnnotations;

namespace RetailRally.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId {  get; set; }
    public User? User { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public double? TotalPrice {  get; set; }
    public int ShippingTypeId {  get; set; }
    [Required(ErrorMessage ="Виберіть тип доставки!")]
    public ShippingType? ShippingType { get; set; }
    public int PaymentTypeId {  get; set; }
    [Required(ErrorMessage ="Виберіть тип оплати!")]
    public PaymentType? PaymentType { get; set; }
    public Status Status { get; set; }
    public DeliveryAddress? DeliveryAddress { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
    public List<OrderProduct>? OrderProducts {  get; set; } = new List<OrderProduct>();
}
