using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Interfaces;
using RetailRally.Models;
using RetailRally.ViewModels;

namespace RetailRally.Controllers;
public class ProductController(IProductRepository _repository, IHttpContextAccessor _httpContextAccessor,
    HubContextClass _context, IDistributedCache _cache) : Controller
{
    public async Task<IActionResult> FilterBy(string filterName, int pageNumber = 1, int pageSize = 10)
    {
        var cacheKey = $"filteredProducts_{filterName}_{pageNumber}_{pageSize}";
        List<Product> products;

        var cachedProducts = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedProducts))
        {
            products = JsonConvert.DeserializeObject<List<Product>>(cachedProducts);
        }
        else
        {
            products = filterName switch
            {
                "Name" => await _context.Products.OrderBy(p => p.Name)
                                                 .Skip((pageNumber - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .Include(p => p.Category)
                                                 .Include(p => p.Comments)
                                                 .ToListAsync(),
                "Price" => await _context.Products.OrderBy(p => p.Price)
                                                  .Skip((pageNumber - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .Include(p => p.Category)
                                                  .Include(p => p.Comments)
                                                  .ToListAsync(),
                _ => new List<Product>(),
            };

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

        var categories = await _repository.GetAllCategoriesAsync();
        var model = new MainPageVm()
        {
            Products = products,
            Categories = categories
        };
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.FilterName = filterName;

        return View("MainPage", model);
    }

    public async Task<IActionResult> FilterByPrice(double priceFrom, double priceTo, int pageNumber = 1, int pageSize = 10)
    {
        var cacheKey = $"filteredProductsByPrice_{priceFrom}_{priceTo}_{pageNumber}_{pageSize}";
        List<Product> products;

        var cachedProducts = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedProducts))
        {
            products = JsonConvert.DeserializeObject<List<Product>>(cachedProducts);
        }
        else
        {
            products = await _context.Products
                .Where(p => p.Price >= priceFrom && p.Price <= priceTo)
                .OrderBy(p => p.Price)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .Include(p => p.Comments)
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

        var categories = await _repository.GetAllCategoriesAsync();
        var model = new MainPageVm()
        {
            Products = products,
            Categories = categories
        };
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.PriceFrom = priceFrom;
        ViewBag.PriceTo = priceTo;

        return View("MainPage", model);
    }


    public async Task<IActionResult> AllProducts(int pageNumber = 1, int pageSize = 10)
    {
        var userId = User.GetUserId();
        var products = await _repository.GetAllProductsAsync(pageNumber, pageSize);
        var cartItems = await _repository.GetCartItemsByUserIdAsync(userId);
        var categories = await _repository.GetAllCategoriesAsync();
        ViewBag.CartItems = cartItems;
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.CategoryId = 0;
        ViewBag.PriceFrom = 0;
        ViewBag.PriceTo = 0;
        ViewBag.FilteringName = null;
        var model = new MainPageVm()
        {
            Products = products,
            Categories = categories
        };
        return View("MainPage", model);
    }

    public async Task<IActionResult> GoToMyProductsPage()
    {
        var userId = User.GetUserId();
        var products = await _repository.GetAllUserProductsAsync(userId);
        return View("MyProductsPage", products);
    }
    public async Task<IActionResult> Search(string term)
    {
        var matchingProducts = await _repository.GetAllSearchedProductsAsync(term);

        return Json(matchingProducts);
    }

    public async Task<IActionResult> DisplayCategoryProducts(int categoryId, int pageNumber = 1, int pageSize = 10)
    {
        var products = await _repository.GetCategoryProductsAsync(categoryId, pageNumber, pageSize);
        var categories = await _repository.GetAllCategoriesAsync();
        var model = new MainPageVm()
        {
            Products = products,
            Categories = categories
        };
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.CategoryId = categoryId;
        return View("MainPage", model);
    }


    [HttpPost]
    public async Task<IActionResult> EditProduct(Product product, IFormFile profilePicture)
    {
        ModelState.Remove("profilePicture");
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors });
        }

        var exisitngProduct = await _repository.GetProductByIdAsync(product.Id);
        exisitngProduct.Name = product.Name;
        exisitngProduct.Description = product.Description;
        exisitngProduct.Price = product.Price;
        exisitngProduct.Quantity = product.Quantity;
        exisitngProduct.CategoryId = product.CategoryId;
        await _repository.UpdateProductAsync(exisitngProduct, profilePicture);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        if (!await _repository.DeleteProductAsync(productId))
        {
            return View();
        }
        return RedirectToAction(nameof(GoToMyProductsPage));
    }

    public async Task<IActionResult> GoToProductEditPage(int productId)
    {
        var product = await _repository.GetProductByIdAsync(productId);
        var categories = await _repository.GetAllCategoriesAsync();
        ViewData["Categories"] = categories.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
        return View("ProductEditPage", product);
    }

    public async Task<IActionResult> ViewPostProductPage()
    {
        var categories = await _repository.GetAllCategoriesAsync();
        ViewData["Categories"] = categories.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
        return View("PostProductPage");
    }

    [HttpPost]
    public async Task<IActionResult> PostProduct(Product product, IFormFile imageFile)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        product.UserId = userId;
        ModelState.Remove("UserId");
        if (!ModelState.IsValid)
        {
            var categories = await _repository.GetAllCategoriesAsync();
            ViewData["Categories"] = categories.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            return View("PostProductPage", product);
        }
        await _repository.AddProductAsync(product, imageFile);
        return RedirectToAction(nameof(AllProducts));
    }

    public async Task<IActionResult> GoToDetailsPage(int productId)
    {
        var product = await _repository.GetProductByIdAsync(productId);
        var productVm = new ProductVm()
        {
            Product = product,
            Comments = product.Comments,
            User = product.User
        };
        return View("DetailsPage", productVm);
    }

    public async Task<IActionResult> GoToBuyPage(List<int> selectedItems, List<int> quantities)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        if (userId == null)
        {
            return View("_LoginPartial");
        }

        var products = new List<Product>();
        var productQuantities = new Dictionary<int, int>();

        for (int i = 0; i < selectedItems.Count; i++)
        {
            var productId = selectedItems[i];
            var quantity = quantities[i];

            var product = await _repository.GetProductByIdAsync(productId);
            if (product != null)
            {
                products.Add(product);
                productQuantities.Add(productId, quantity);
            }
        }

        var user = await _repository.GetUserByIdAsync(userId);
        var paymentTypes = await _repository.GetAllPaymentTypesAsync();
        var shippingTypes = await _repository.GetAllShippingTypesAsync();

        ViewData["ShippingTypes"] = new SelectList(shippingTypes, "Id", "Name");
        ViewData["PaymentTypes"] = new SelectList(paymentTypes, "Id", "Name");

        var buyProductVm = new BuyProductVm()
        {
            Products = products,
            User = user,
            ProductQuantities = productQuantities
        };

        return View("BuyPage", buyProductVm);
    }


    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return View("_LoginPartial");
        }

        var cart = await _context.Carts.Include(c => c.Items)
                                       .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>()
            };
            await _repository.AddCartAsync(cart);
        }

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

        cartItem = new CartItem
        {
            CartId = cart.CartId,
            ProductId = productId,
        };
        await _repository.AddItemToCartAsync(cartItem);

        await _repository.SaveChangesAsync();

        return RedirectToAction(nameof(AllProducts));
    }

    public async Task<IActionResult> GoToMyCartPage()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return View("_LoginPartial");
        }

        var cartItems = await _repository.GetCartItemsWithProductsByUserIdAsync(userId);

        return View("MyCart", cartItems);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveProductFromCart(int productId)
    {
        var userId = User.GetUserId();

        if (productId == null)
        {
            return RedirectToAction(nameof(GoToMyCartPage));
        }
        var cart = await _repository.GetCartByUserIdAsync(userId);
        if (cart != null)
        {
            var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                await _repository.RemoveCartItem(itemToRemove);
            }
        }
        await _repository.SaveChangesAsync();
        return RedirectToAction(nameof(GoToMyCartPage));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveSelectedFromCart(int[] selectedItems)
    {
        var userId = User.GetUserId();

        if (selectedItems == null || selectedItems.Length == 0)
        {
            return RedirectToAction(nameof(GoToMyCartPage));
        }
        var cart = await _repository.GetCartByUserIdAsync(userId);
        foreach (var itemId in selectedItems)
        {
            if (cart != null)
            {
                var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == itemId);
                if (itemToRemove != null)
                {
                    await _repository.RemoveCartItem(itemToRemove);
                }
            }
        }
        await _repository.SaveChangesAsync();
        return RedirectToAction(nameof(GoToMyCartPage));
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(BuyProductVm model, int[] productIds, int[] quantities)
    {
        ModelState.Remove("Order.UserId");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.GetUserId();

        var createdOrder = await _repository.CreateOrderAsync(model.Order, userId);

        if (createdOrder == null)
        {
            ModelState.AddModelError("", "Hе вдалося створити замовлення. Будь ласка, спробуйте ще раз.");
            return View(model);
        }

        var cart = await _repository.GetCartByUserIdAsync(userId);
        for (int i = 0; i < productIds.Length; i++)
        {
            var productId = productIds[i];
            var quantity = quantities[i];
            await _repository.AddProductToOrderAsync(createdOrder.Id, productId, quantity);
            if (cart != null)
            {
                var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (itemToRemove != null)
                {
                    await _repository.RemoveCartItem(itemToRemove);
                }
            }
        }
        await _repository.SaveChangesAsync();
        if (model.DeliveryAddress.Country != null)
        {
            model.DeliveryAddress.OrderId = createdOrder.Id;
            var addressSaved = await _repository.AddDeliveryAddressAsync(model.DeliveryAddress);
            if (!addressSaved)
            {
                ModelState.AddModelError("", "Виникла проблема зі збереженням адреси доставки.");
                return View(model);
            }
            return RedirectToAction("OrderSummary", new { orderId = createdOrder.Id });
        }

        return RedirectToAction("OrderSummary", new { orderId = createdOrder.Id });
    }

    public async Task<IActionResult> OrderSummary(int orderId)
    {
        var order = await _repository.GetOrderSummaryAsync(orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View("OrderSummaryPage", order);
    }

    public async Task<IActionResult> MyOrders()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return View("_LoginPartial");
        }

        var orders = await _repository.GetAllUserOrdersAsync(userId);

        return View("MyOrdersPage", orders);
    }

    public async Task<IActionResult> GoToOrdersFromMyShopPage()
    {
        var userId = User.GetUserId();
        var orders = await _repository.GetOrdersFromMyShopAsync(userId);
        ViewBag.OrderStatuses = Enum.GetValues(typeof(Status))
                                .Cast<Status>()
                                .Select(s => new SelectListItem
                                {
                                    Text = s.ToString(),
                                    Value = ((int)s).ToString()
                                })
                                .ToList();
        return View("OrdersFromShop", orders);
    }

    [HttpPost]
    public async Task<IActionResult> EditOrderStatus(int orderId, string newStatus)
    {
        if (!Enum.TryParse(newStatus, out Status status))
        {
            return BadRequest("Недійсний статус.");
        }

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return NotFound($"Замовлення з ідентифікатором {orderId} не знайдено.");
        }

        order.Status = status;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(GoToOrdersFromMyShopPage));
    }
}
