using RetailRally.Models;

namespace RetailRally.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<List<Product>> GetAllSearchedProductsAsync(string term);
    Task<List<Product>> GetAllCategoryProductsAsync(int categoryId);
    Task<List<Product>> GetAllUserProductsAsync(string userId);
    Task<List<Order>> GetAllUserOrdersAsync(string userId);
    Task<Order> GetOrderSummaryAsync(int orderId);
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<ShippingType>> GetAllShippingTypesAsync();
    Task<List<PaymentType>> GetAllPaymentTypesAsync();
    Task<List<CartItem>> GetCartItemsByUserIdAsync(string userId);
    Task<List<CartItem>> GetCartItemsWithProductsByUserIdAsync(string userId);
    Task<Order> CreateOrderAsync(Order order, string userId);
    Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity = 1);
    Task<bool> AddDeliveryAddressAsync(DeliveryAddress address);
    Task<bool> DeleteProductAsync(int productId);
    Task<bool> UpdateProductAsync(Product newProduct, IFormFile profilePicture);
    Task<bool> AddCartAsync(Cart cart);
    Task AddItemToCartAsync(CartItem cartItem);
    Task<int> GetNumberOfProductsInCart(string userId);
    Task<Cart> GetCartByUserIdAsync(string userId);
    Task<bool> RemoveCartItem(CartItem cartItem);
    Task<Product> GetProductByIdAsync(int id);
    Task<User> GetUserByIdAsync(string id);
    Task<bool> AddProductAsync(Product product, IFormFile image);
    Task<bool> SaveChangesAsync();
}
