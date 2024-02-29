﻿using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Interfaces;
using RetailRally.Models;

namespace RetailRally.Repositories;

public class ProductRepository(HubContextClass _db, IConfiguration _configuration,
    AzureStorageService _service, UserManager<User> _userManager) : IProductRepository
{
    private readonly string _containerName = _configuration["AzureStorageConfig:ProductsImageContainer"];
    public async Task<bool> AddProductAsync(Product product, IFormFile image)
    {
        if (image != null && image.Length > 0)
        {
            using (var stream = image.OpenReadStream())
            {
                var imageUrl = await _service.UploadImageAsync(stream, image.FileName, _containerName);
                product.PictureUrl = imageUrl;
            }
        }
        await _db.Products.AddAsync(product);
        return await SaveChangesAsync();
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _db.Categories.ToListAsync();
    }

    public async Task<List<PaymentType>> GetAllPaymentTypesAsync()
    {
        return await _db.PaymentTypes.ToListAsync();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _db.Products.Include(p => p.Category).ToListAsync();
    }

    public async Task<List<ShippingType>> GetAllShippingTypesAsync()
    {
        return await _db.ShippingTypes.ToListAsync();
    }
    public async Task<List<CartItem>> GetCartItemsWithProductsByUserIdAsync(string userId)
    {
        return await _db.CartItems
            .Where(ci => ci.Cart.UserId == userId)
            .Include(ci => ci.Product)
            .ToListAsync();
    }

    public async Task<List<CartItem>> GetCartItemsByUserIdAsync(string userId)
    {
        return await _db.Carts
                        .Include(c => c.Items)
                        .Where(c => c.UserId == userId)
                        .SelectMany(c => c.Items)
                        .ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Include(p => p.User)
            .Include(p => p.Comments.OrderByDescending(c => c.Id))
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<bool> SaveChangesAsync()
    {
        var saved = await _db.SaveChangesAsync();
        return saved > 0 ? true : false;
    }

    public async Task<Cart> GetCartByUserIdAsync(string userId)
    {
        return await _db.Carts.Include(c => c.Items).Where(u => u.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<bool> RemoveCartItem(CartItem cartItem)
    {
        _db.Entry(cartItem).State = EntityState.Deleted;
        return true;
    }

    public async Task<int> GetNumberOfProductsInCart(string userId)
    {
        var cart = await _db.Carts
                         .Include(c => c.Items)
                         .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items == null)
        {
            return 0;
        }

        return cart.Items.Sum(item => item.Quantity);
    }

    public async Task<Order> CreateOrderAsync(Order order, string userId)
    {
        order.UserId = userId;
        order.Status = Status.Pending;
        order.TotalPrice = 0;

        _db.Orders.Add(order);
        await SaveChangesAsync();
        return order;
    }

    public async Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity = 1)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null)
        {
            return false;
        }

        var orderProduct = new OrderProduct
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity
        };

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }
        order.TotalPrice += product.Price * quantity;

        _db.OrderProducts.Add(orderProduct);
        return await SaveChangesAsync();
    }

    public async Task<bool> AddDeliveryAddressAsync(DeliveryAddress address)
    {
        _db.DeliveryAddresses.Add(address);
        return await SaveChangesAsync();
    }

    public async Task<bool> AddCartAsync(Cart cart)
    {
        await _db.Carts.AddAsync(cart);
        return await SaveChangesAsync();
    }

    public async Task AddItemToCartAsync(CartItem cartItem)
    {
        await _db.CartItems.AddAsync(cartItem);
    }

    public async Task<List<Order>> GetAllUserOrdersAsync(string userId)
    {
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
            .Include(op => op.DeliveryAddress)
            .Include(o => o.ShippingType)
            .Include(o => o.PaymentType)
            .ToListAsync();
    }

    public async Task<Order> GetOrderSummaryAsync(int orderId)
    {
        return await _db.Orders
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
            .Include(o => o.ShippingType)
            .Include(o => o.PaymentType)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Product>> GetAllUserProductsAsync(string userId)
    {
        return await _db.Products.Include(p => p.Category).Where(o => o.UserId == userId).ToListAsync();
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        using var transaction = _db.Database.BeginTransaction();
        try
        {
            var orderProducts = await _db.OrderProducts
                                     .Where(op => op.ProductId == productId)
                                     .Include(op => op.Order)
                                     .ToListAsync();

            foreach (var orderProduct in orderProducts)
            {
                var order = orderProduct.Order;
                if (order != null)
                {
                    order.Status = Status.ProductDeleted;
                    _db.Orders.Update(order);
                }
            }

            _db.OrderProducts.RemoveRange(orderProducts);
            await _db.SaveChangesAsync();

            var cartProducts = await _db.CartItems
                                     .Where(c => c.ProductId == productId)
                                     .ToListAsync();

            _db.CartItems.RemoveRange(cartProducts);
            var product = await _db.Products.FindAsync(productId);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.PictureUrl))
                {
                    await _service.DeleteImageAsync(product.PictureUrl, _containerName);
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(Product newProduct, IFormFile profilePicture)
    {
        if (profilePicture != null && profilePicture.Length > 0)
        {
            if (!string.IsNullOrEmpty(newProduct.PictureUrl))
            {
                await _service.DeleteImageAsync(newProduct.PictureUrl, _containerName);
            }

            using (var stream = profilePicture.OpenReadStream())
            {
                var imageUrl = await _service.UploadImageAsync(stream, profilePicture.FileName, _containerName);
                newProduct.PictureUrl = imageUrl;
            }
        }
        _db.Products.Update(newProduct);
        return await SaveChangesAsync();
    }

    public async Task<List<Product>> GetAllCategoryProductsAsync(int categoryId)
    {
        return await _db.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
    }

    public async Task<List<Product>> GetAllSearchedProductsAsync(string term)
    {
        return await _db.Products
             .Where(p => p.Name.StartsWith(term))
             .ToListAsync();
    }
}