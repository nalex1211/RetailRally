using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Interfaces;
using RetailRally.Models;
using RetailRally.ViewModels;

namespace RetailRally.Repositories;

public class AdminRepository(HubContextClass _context, IProductRepository _productRepository) : IAdminRepository
{
    public async Task<bool> AddPaymentTypeAsync(PaymentType paymentType)
    {
        await _context.PaymentTypes.AddAsync(paymentType);
        return await SaveChangesAsync();
    }

    public async Task<bool> AddShippingTypeAsync(ShippingType shippingType)
    {
        await _context.ShippingTypes.AddAsync(shippingType);
        return await SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _context.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return false;
        }

        if (user.Products != null)
        {
            var productsToDelete = user.Products.ToList();

            foreach (var product in productsToDelete)
            {
                await _productRepository.DeleteProductAsync(product.Id);
            }
        }

        _context.Users.Remove(user);
        return await SaveChangesAsync();
    }

    public async Task<List<PaymentType>> GetAllPaymentTypesAsync()
    {
        return await _context.PaymentTypes.ToListAsync();
    }

    public async Task<List<ShippingType>> GetAllShippingTypesAsync()
    {
        return await _context.ShippingTypes.ToListAsync();
    }

    public async Task<List<AllUserVm>> GetAllUsersAsync()
    {
        var usersWithRoles = await (from user in _context.Users
                                    select new AllUserVm
                                    {
                                        Id = user.Id,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Email = user.Email,
                                        UserName = user.UserName,
                                        Roles = (from userRole in _context.UserRoles
                                                 join role in _context.Roles on userRole.RoleId equals role.Id
                                                 where userRole.UserId == user.Id
                                                 select role.Name).ToList()
                                    }).ToListAsync();

        return usersWithRoles;
    }


    public async Task<bool> SaveChangesAsync()
    {
        var saved = await _context.SaveChangesAsync();
        return saved > 0 ? true : false;
    }
}
