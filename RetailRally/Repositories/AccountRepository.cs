using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Interfaces;
using RetailRally.Models;
using RetailRally.ViewModels;
using System;
using static RetailRally.Helpers.Roles;

namespace RetailRally.Repositories;

public class AccountRepository(HubContextClass _db,
                               UserManager<User> _userManager,
                               SignInManager<User> _signInManager,
                               AzureStorageService _service,
                               IConfiguration _configuration,
                               IHttpContextAccessor _httpContextAccessor,
                               EmailService _emailService,
                               IUrlHelperFactory _urlHelperFactory,
                               IActionContextAccessor _actionContextAccessor) : IAccountRepository
{
    private readonly string _containerName = _configuration["AzureStorageConfig:ImageContainer"];
    public async Task<bool> CheckEmailValidation(string email)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        return user.Email == email ? true : false;
    }

    public async Task<bool> CheckIfCorrectPasswordAsync(string password, User user)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> CheckIfEmailExistsAsync(string email)
    {
        var userWithEmailExists = await _db.Users.AnyAsync(u => u.Email == email);
        return userWithEmailExists;
    }

    public async Task<bool> CheckIfUserExistsAsync(string id)
    {
        return await _db.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> CheckIfUsernameExistsAsync(string username)
    {
        var userWithUsernameExists = await _db.Users.AnyAsync(u => u.UserName == username);
        return userWithUsernameExists;
    }

    public async Task<bool> ConfirmEmailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return false;
        }
        return true;
    }

    public async Task<bool> ConfirmNewEmailAsync(NewEmailVm model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
        var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);

        if (!result.Succeeded)
        {
            return false;
        }
        user.EmailConfirmed = false;
        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);
        return true;
    }

    public async Task<bool> ConfirmNewPasswordAsync(NewPasswordVm model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
        {
            return false;
        }
        return true;
    }

    public async Task<IdentityResult> CreateUser(User user, string password, IFormFile userImage = null, string pictureUrl = null)
    {
        if (userImage != null && userImage.Length > 0)
        {
            using (var stream = userImage.OpenReadStream())
            {
                var uploadedImageUrl = await _service.UploadImageAsync(stream, userImage.FileName, _containerName);
                user.PictureUrl = uploadedImageUrl;
            }
        }
        else if (!string.IsNullOrWhiteSpace(pictureUrl))
        {
            user.PictureUrl = pictureUrl;
        }
        else
        {
            user.PictureUrl = _configuration["AzureStorageConfig:DefaultIconUrl"];
        }

        IdentityResult createResult;
        if (password != null)
        {
            createResult = await _userManager.CreateAsync(user, password);
        }
        else
        {
            createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Role.Customer);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return createResult;
            }
        }

        if (createResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Role.Customer);
            await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);
            return createResult;
        }
        return createResult;
    }

    public async Task<User> FindUserbyEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> LogIn(User user, string password, bool rememberMe)
    {
        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
        return result.Succeeded ? true : false;
    }

    public async Task<bool> SendEmailChangeLinkAsync(string email)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        var callbackUrl = urlHelper.Action(
            action: "NewEmailChange",
            controller: "Account",
            values: new { userId = userId, email = email },
            protocol: _httpContextAccessor.HttpContext.Request.Scheme);

        string messageText = "Якщо ви хочете змінити свою електронну адресу, натисніть кнопку нижче:";
        await _emailService.SendEmailAsync(email, "Email change", callbackUrl, messageText, "Електронну пошту");

        return true;
    }

    public async Task<bool> SendEmailConfirmationLinkAsync(string email)
    {
        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        var callbackUrl = urlHelper.Action(
            action: "ConfirmEmail",
            controller: "Account",
            values: new { userId },
            protocol: _httpContextAccessor.HttpContext.Request.Scheme);

        string messageText = "Якщо ви хочете підтвердити свій обліковий запис, натисніть кнопку нижче:";
        await _emailService.SendEmailAsync(email, "Email confirmation", callbackUrl, messageText, "Електронну пошту");

        return true;
    }

    public async Task<bool> SendPasswordChangeLinkAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var callbackUrl = urlHelper.Action(
            action: "NewPasswordChange",
            controller: "Account",
            values: new { userId = user.Id },
            protocol: _httpContextAccessor.HttpContext.Request.Scheme);

        string messageText = "Якщо ви хочете змінити пароль, натисніть кнопку нижче:";
        await _emailService.SendEmailAsync(email, "Password change", callbackUrl, messageText, "Пароль");

        return true;
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<IEnumerable<string>> ValidatePasswordAsync(string password)
    {
        var passwordValidator = _userManager.PasswordValidators.First();
        var result = await passwordValidator.ValidateAsync(_userManager, null, password);

        if (result.Succeeded)
        {
            return Enumerable.Empty<string>();
        }

        return result.Errors.Select(error => error.Description);
    }
}
