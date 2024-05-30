using Microsoft.AspNetCore.Identity;
using RetailRally.Contexts;
using RetailRally.Models;
using static RetailRally.Helpers.Roles;

namespace RetailRally.Utilities;

public static class InitializationToDb
{
    public static async Task AddRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!_roleManager.Roles.Any())
        {
            foreach (var roleName in Role.AllRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole { Name = roleName };
                    await _roleManager.CreateAsync(role);
                }
            }
        }
    }
    public static async Task AddCategoriesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var _db = serviceProvider.GetRequiredService<HubContextClass>();
        if (!_db.Categories.Any())
        {
            var categories = new List<Category>()
            {
                new Category{Name="Електроніка"},
                new Category{Name="Дім і сад"},
                new Category{Name="Сім'я"},
                new Category{Name="Ігри"},
                new Category{Name="Краса"}
            };
            await _db.Categories.AddRangeAsync(categories);
            await _db.SaveChangesAsync();
        }
    }

    public static async Task AddShippingAndPaymentTypesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var _db = serviceProvider.GetRequiredService<HubContextClass>();
        if (!_db.ShippingTypes.Any())
        {
            var shippingTypes = new List<ShippingType>()
            {
                new ShippingType{Name="Нова Пошта"},
                new ShippingType{Name="Укрпошта"},
                new ShippingType{Name="Заберу сам"}
            };
            await _db.ShippingTypes.AddRangeAsync(shippingTypes);
            await _db.SaveChangesAsync();
        }
        if (!_db.PaymentTypes.Any())
        {
            var paymentTypes = new List<PaymentType>()
            {
                new PaymentType{Name="Приват24"},
                new PaymentType{Name="Monobank"},
                new PaymentType{Name="Готівка"},
                new PaymentType{Name="PayPal"}
            };
            await _db.PaymentTypes.AddRangeAsync(paymentTypes);
            await _db.SaveChangesAsync();
        }
    }

    public static async Task AddAdminUser(this WebApplication app, IConfiguration configuration)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var _userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var adminUser = await _userManager.FindByEmailAsync(configuration["AdminInfo:Email"]);
        if (adminUser is null)
        {
            adminUser = new User()
            {
                Email = configuration["AdminInfo:Email"],
                FirstName = configuration["AdminInfo:FirstName"],
                LastName = configuration["AdminInfo:LastName"],
                PhoneNumber = configuration["AdminInfo:PhoneNumber"],
                UserName = configuration["AdminInfo:UserName"],
                BirthDate = new DateTime(2003, 12, 5),
                PictureUrl = configuration["AzureStorageConfig:DefaultIconUrl"]
            };
            await _userManager.CreateAsync(adminUser, configuration["AdminInfo:Password"]);
        }

        if (!await _userManager.IsEmailConfirmedAsync(adminUser))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(adminUser);
            await _userManager.ConfirmEmailAsync(adminUser, token);

            await _userManager.AddToRoleAsync(adminUser, Role.Admin);
        }
    }
}
