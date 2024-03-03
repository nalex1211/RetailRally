using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Interfaces;
using RetailRally.Models;
using RetailRally.Repositories;
using RetailRally.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<AzureStorageService>();
builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration["SignalRConnString"]);

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddDbContext<HubContextClass>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarketplaceDbConnection"));
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 5;

}).AddEntityFrameworkStores<HubContextClass>().AddDefaultTokenProviders();

builder.Services.AddAuthentication().AddCookie().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["GoogleCredentials:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["GoogleCredentials:ClientSecret"];
    googleOptions.Scope.Add("profile");
    googleOptions.ClaimActions.MapJsonKey("image", "picture");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();
app.UseAzureSignalR(routes =>
{
    routes.MapHub<ChatHub>("/chatHub");
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=AllProducts}/{id?}");
if (app.Environment.IsDevelopment())
{
    await app.AddRolesAsync();
    await app.AddCategoriesAsync();
    await app.AddAdminUser(app.Configuration);
    await app.AddShippingAndPaymentTypesAsync();
}
app.Run();
