using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Interfaces;
using RetailRally.Models;

namespace RetailRally.Repositories;

public class ProductRepository(HubContextClass _context, IConfiguration _configuration,
    AzureStorageService _service, UserManager<User> _userManager, IDistributedCache _cache) : IProductRepository
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
        await _context.Products.AddAsync(product);
        await InvalidateProductCache();
        return await SaveChangesAsync();
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<List<PaymentType>> GetAllPaymentTypesAsync()
    {
        return await _context.PaymentTypes.ToListAsync();
    }

    public async Task<List<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
    {
        var cacheVersionKey = "productsCacheVersion";
        var cacheKey = $"products_{pageNumber}_{pageSize}_v{await GetCacheVersionAsync(cacheVersionKey)}";
        List<Product> products;

        var cachedProducts = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedProducts))
        {
            products = JsonConvert.DeserializeObject<List<Product>>(cachedProducts);
        }
        else
        {
            products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(products, jsonSettings), cacheOptions);
        }

        return products;
    }


    private async Task<string> GetCacheVersionAsync(string cacheVersionKey)
    {
        var cacheVersion = await _cache.GetStringAsync(cacheVersionKey);
        return cacheVersion ?? "1";
    }

    public async Task<List<ShippingType>> GetAllShippingTypesAsync()
    {
        return await _context.ShippingTypes.ToListAsync();
    }
    public async Task<List<CartItem>> GetCartItemsWithProductsByUserIdAsync(string userId)
    {
        return await _context.CartItems
            .Where(ci => ci.Cart.UserId == userId)
            .Include(ci => ci.Product)
            .ThenInclude(p => p.User)
            .ToListAsync();
    }

    public async Task<List<CartItem>> GetCartItemsByUserIdAsync(string userId)
    {
        return await _context.Carts
                        .Include(c => c.Items)
                        .Where(c => c.UserId == userId)
                        .SelectMany(c => c.Items)
                        .ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _context.Products
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
        var saved = await _context.SaveChangesAsync();
        return saved > 0 ? true : false;
    }

    public async Task<Cart> GetCartByUserIdAsync(string userId)
    {
        return await _context.Carts.Include(c => c.Items).Where(u => u.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<bool> RemoveCartItem(CartItem cartItem)
    {
        _context.Entry(cartItem).State = EntityState.Deleted;
        return true;
    }

    public async Task<int> GetNumberOfProductsInCart(string userId)
    {
        var cart = await _context.Carts
                         .Include(c => c.Items)
                         .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items == null)
        {
            return 0;
        }

        return cart.Items.Count();
    }

    public async Task<Order> CreateOrderAsync(Order order, string userId)
    {
        order.UserId = userId;
        order.Status = Status.Pending;
        order.TotalPrice = 0;

        _context.Orders.Add(order);
        await SaveChangesAsync();
        return order;
    }

    public async Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
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
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }
        if (product.Discount > 0)
        {
            order.TotalPrice += product.DiscountedPrice * quantity;
        }
        else
        {
            order.TotalPrice += product.Price * quantity;
        }
        product.Quantity -= quantity;
        await UpdateProductAsync(product, null);
        _context.OrderProducts.Add(orderProduct);
        return await SaveChangesAsync();
    }

    public async Task<bool> AddDeliveryAddressAsync(DeliveryAddress address)
    {
        _context.DeliveryAddresses.Add(address);
        return await SaveChangesAsync();
    }

    public async Task<bool> AddCartAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        return await SaveChangesAsync();
    }

    public async Task AddItemToCartAsync(CartItem cartItem)
    {
        await _context.CartItems.AddAsync(cartItem);
    }

    public async Task<Order> GetOrderSummaryAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
            .Include(o => o.ShippingType)
            .Include(o => o.PaymentType)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Product>> GetAllUserProductsAsync(string userId)
    {
        return await _context.Products.Include(p => p.Category).Where(o => o.UserId == userId).ToListAsync();
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var orderProducts = await _context.OrderProducts
                                     .Where(op => op.ProductId == productId)
                                     .Include(op => op.Order)
                                     .ToListAsync();

            foreach (var orderProduct in orderProducts)
            {
                var order = orderProduct.Order;
                if (order != null)
                {
                    order.Status = Status.ProductDeleted;
                    _context.Orders.Update(order);
                }
            }

            _context.OrderProducts.RemoveRange(orderProducts);
            await _context.SaveChangesAsync();

            var cartProducts = await _context.CartItems
                                     .Where(c => c.ProductId == productId)
                                     .ToListAsync();

            _context.CartItems.RemoveRange(cartProducts);
            var comments = await _context.Comments.Where(c => c.ProductId == productId).ToListAsync();
            if (comments != null)
            {
                _context.Comments.RemoveRange(comments);
                await _context.SaveChangesAsync();
            }
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.PictureUrl))
                {
                    await _service.DeleteImageAsync(product.PictureUrl, _containerName);
                }

                _context.Products.Remove(product);
                await InvalidateProductCache();
                await _context.SaveChangesAsync();
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

    private async Task InvalidateProductCache()
    {
        var cacheVersionKey = "productsCacheVersion";
        var currentVersion = await _cache.GetStringAsync(cacheVersionKey);
        var newVersion = (int.Parse(currentVersion ?? "1") + 1).ToString();

        await _cache.SetStringAsync(cacheVersionKey, newVersion);
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
        await InvalidateProductCache();
        _context.Products.Update(newProduct);
        return await SaveChangesAsync();
    }

    public async Task<List<Product>> GetCategoryProductsAsync(int categoryId, int pageNumber, int pageSize)
    {
        var cacheKey = $"categoryProducts_{categoryId}_{pageNumber}_{pageSize}";
        List<Product> products;

        var cachedProducts = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedProducts))
        {
            products = JsonConvert.DeserializeObject<List<Product>>(cachedProducts);
        }
        else
        {
            products = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(products, jsonSettings), cacheOptions);
        }

        return products;
    }


    public async Task<List<Product>> GetAllSearchedProductsAsync(string term)
    {
        return await _context.Products
             .Where(p => p.Name.StartsWith(term))
             .ToListAsync();
    }

    public async Task<List<Order>> GetAllUserOrdersAsync(string userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
            .Include(op => op.DeliveryAddress)
            .Include(o => o.ShippingType)
            .Include(o => o.PaymentType)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersFromMyShopAsync(string userId)
    {
        return await _context.Orders
            .Where(o => o.Products.Any(p => p.UserId == userId))
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
            .Include(op => op.DeliveryAddress)
            .Include(o => o.ShippingType)
            .Include(o => o.PaymentType)
            .Include(o => o.User)
            .ToListAsync();
    }

    public async Task<ShippingType> GetShippingTypeByIdAsync(int id)
    {
        return await _context.ShippingTypes.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<PaymentType> GetPaymentTypeByIdAsync(int id)
    {
        return await _context.PaymentTypes.FirstOrDefaultAsync(p => p.Id == id);
    }
}
