﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Models;
using RetailRally.ViewModels;
using System.Text.Encodings.Web;
using static RetailRally.Helpers.Roles;

namespace RetailRally.Controllers;
public class UserController(HubContextClass _db, IHttpContextAccessor _httpContextAccessor,
    UserManager<User> _userManager, AzureStorageService _service, IConfiguration _configuration,
    EmailService _emailService, SignInManager<User> _signInManager, RoleManager<IdentityRole> _roleManager) : Controller
{
    private readonly string _containerName = _configuration["AzureStorageConfig:ImageContainer"];
    public async Task<IActionResult> MyProfile()
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        var role = await _userManager.GetRolesAsync(user);
        var model = new UserVm()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            BirthDate = user?.BirthDate,
            PictureUrl = user.PictureUrl,
            UserName = user.UserName,
            Email = user.Email,
            Role = role.FirstOrDefault()
        };
        return View("ProfilePage", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile([FromForm] UserVm model)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, errors = new[] { new { description = $"Не вдається завантажити користувача з ідентифікатором '{userId}'." } } });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { description = e.ErrorMessage }).ToArray();
            return Json(new { success = false, errors });
        }

        if (user.UserName != model.UserName && await _userManager.FindByNameAsync(model.UserName) != null)
        {
            ModelState.AddModelError("UserName", "Це ім'я користувача вже зайнято.");
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { description = e.ErrorMessage }).ToArray();
            return Json(new { success = false, errors });
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.UserName = model.UserName;
        try
        {
            await _userManager.UpdateAsync(user);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Неможливо зберегти зміни.");
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => new { description = e.ErrorMessage }).ToArray();
            return Json(new { success = false, errors });
        }

        return Json(new { success = true });
    }

    private UserVm MapUserToViewModel(User user)
    {
        return new UserVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            BirthDate = user.BirthDate,
            PictureUrl = user.PictureUrl,
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeProfilePicture(IFormFile profilePicture)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        var defaultPictureUrl = _configuration["AzureStorageConfig:DefaultIconUrl"];

        if (profilePicture != null && profilePicture.Length > 0)
        {
            if (!string.IsNullOrEmpty(user.PictureUrl) && user.PictureUrl != defaultPictureUrl)
            {
                await _service.DeleteImageAsync(user.PictureUrl, _containerName);
            }

            using var stream = profilePicture.OpenReadStream();
            user.PictureUrl = await _service.UploadImageAsync(stream, profilePicture.FileName, _containerName);

            _db.Update(user);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(MyProfile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BecomeVendor()
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (!AreRequiredPropertiesFilled(user))
        {
            return Json(new { success = false, error = "Будь ласка, заповніть всі обов'язкові поля, перш ніж стати продавцем." });
        }

        await _userManager.AddToRoleAsync(user, "Vendor");
        await _userManager.RemoveFromRoleAsync(user, "Customer");
        await _db.SaveChangesAsync();
        await _signInManager.RefreshSignInAsync(user);
        TempData["VendorChange"] = true;
        return Json(new { success = true });
    }



    public bool AreRequiredPropertiesFilled(User user)
    {
        if (string.IsNullOrWhiteSpace(user.FirstName) ||
         string.IsNullOrWhiteSpace(user.LastName) ||
         string.IsNullOrWhiteSpace(user.PhoneNumber) ||
         string.IsNullOrWhiteSpace(user.Email) ||
         string.IsNullOrWhiteSpace(user.UserName))
        {
            return false;
        }
        return true;
    }
}
