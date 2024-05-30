using RetailRally.Models;
using RetailRally.ViewModels;

namespace RetailRally.Interfaces;

public interface IAdminRepository
{
    Task<List<AllUserVm>> GetAllUsersAsync();
    Task<List<ShippingType>> GetAllShippingTypesAsync();
    Task<List<PaymentType>> GetAllPaymentTypesAsync();
    Task<bool> DeleteUserAsync(string id);
    Task<bool> SaveChangesAsync();
    Task<bool> AddShippingTypeAsync(ShippingType shippingType);
    Task<bool> AddPaymentTypeAsync(PaymentType paymentType);
}
