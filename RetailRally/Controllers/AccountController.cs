using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Interfaces;
using RetailRally.Models;
using RetailRally.Repositories;
using RetailRally.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RetailRally.Controllers;
public class AccountController(IAccountRepository _repository, SignInManager<User> _signInManager, UserManager<User> _userManager) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                e => e.Key,
                e => e.Value.Errors.Select(er => er.ErrorMessage).ToArray()
            );
            return Json(new { success = false, errors = errors });
        }

        var emailExists = await _repository.CheckIfEmailExistsAsync(model.Email);
        if (!emailExists)
        {
            return Json(new { success = false, errors = new { Email = new string[] { "User with this email doesn't exist!" } } });
        }

        var user = await _repository.FindUserbyEmailAsync(model.Email);
        var passwordCheck = await _repository.CheckIfCorrectPasswordAsync(model.Password, user);
        if (!passwordCheck)
        {
            return Json(new { success = false, errors = new { Password = new string[] { "Wrong password!" } } });
        }

        var result = await _repository.LogIn(user, model.Password, model.RememberMe);
        if (result)
        {
            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false, message = "Invalid login attempt." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm model, IFormFile userImage)
    {
        ModelState.Remove("userImage");
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var passwordErrors = await _repository.ValidatePasswordAsync(model.Password);
            if (passwordErrors.Any())
            {
                return Json(new { success = false, errors = new { Password = passwordErrors } });
            }
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                e => e.Key,
                e => e.Value.Errors.Select(er => er.ErrorMessage).ToArray()
            );
            return Json(new { success = false, errors = errors });
        }

        var emailExists = await _repository.CheckIfEmailExistsAsync(model.Email);
        if (emailExists)
        {
            return Json(new { success = false, errors = new { Email = new[] { "This email already exists!" } } });
        }

        var usernameExists = await _repository.CheckIfUsernameExistsAsync(model.Username);
        if (usernameExists)
        {
            return Json(new { success = false, errors = new { Username = new[] { "This username already exists!" } } });
        }
        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            UserName = model.Username,
            BirthDate = model.BirthDate
        };
        var userResponse = await _repository.CreateUser(user, model.Password, userImage);

        if (userResponse.Succeeded)
        {
            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while creating your account." });
        }
    }

    

    [HttpPost]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (remoteError != null)
        {
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var pictureUrl = info.Principal.FindFirst("image").Value;
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };
                var createResult = await _repository.CreateUser(user, null, null, pictureUrl);

                if (createResult.Succeeded)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        TempData["ShowRegisterSuccess"] = true;
                        return RedirectToAction("AllProducts", "Product");
                    }
                    else
                    {
                        foreach (var error in addLoginResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View();
                    }
                }
            }
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View("ExternalLoginFailure");
    }



    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _repository.SignOutAsync();
        TempData["ShowLogoutSuccess"] = true;
        return RedirectToAction("AllProducts", "Product");
    }

    public IActionResult ShowEmailChangePage()
    {
        return View("EmailUpdatePage");
    }

    public IActionResult NewEmailChange(string userId, string email)
    {
        var model = new NewEmailVm { UserId = userId, OldEmail = email };
        return View("NewEmailPage", model);
    }

    [HttpPost]
    public async Task<IActionResult> SendEmailChangeLink(string email)
    {
        if (!ModelState.IsValid)
        {
            return View("EmailUpdatePage");
        }

        if (!await _repository.CheckEmailValidation(email))
        {
            TempData["WrongEmail"] = "Wrong email!";
            return View("EmailUpdatePage");
        }

        await _repository.SendEmailChangeLinkAsync(email);
        TempData["MessageSent"] = true;
        return View("EmailUpdatePage");
    }
    public async Task<IActionResult> ConfirmEmail(string userId)
    {
        if (await _repository.ConfirmEmailAsync(userId))
        {
            TempData["Success"] = true;
            return RedirectToAction("MyProfile", "User");
        }
        return NotFound();
    }
    [HttpPost]
    public async Task<IActionResult> ConfirmNewEmail(NewEmailVm model)
    {
        if (!ModelState.IsValid)
        {
            return View("NewEmailPage", model);
        }

        if (!await _repository.CheckIfUserExistsAsync(model.UserId))
        {
            TempData["UserNotFound"] = "User not found!";
            return View("NewEmailPage", model);
        }

        if (await _repository.CheckIfEmailExistsAsync(model.NewEmail))
        {
            TempData["EmailExists"] = "This email already exists!";
            return View("NewEmailPage", model);
        }

        if (await _repository.ConfirmNewEmailAsync(model))
        {
            TempData["Message"] = true;
            return RedirectToAction("MyProfile", "User");
        }
        return View("NewEmailPage", model);
    }

    public IActionResult ShowPasswordChangePage()
    {
        return View("PasswordUpdatePage");
    }

    [HttpPost]
    public async Task<IActionResult> SendPasswordChangeLink(string email)
    {
        if (!ModelState.IsValid)
        {
            return View("PasswordUpdatePage");
        }

        if (!await _repository.CheckIfEmailExistsAsync(email))
        {
            TempData["NoUser"] = "User with this email doesn't exist!";
            return View("PasswordUpdatePage");
        }

        await _repository.SendPasswordChangeLinkAsync(email);
        TempData["MessageSent"] = true;
        return View("PasswordUpdatePage");
    }

    public IActionResult NewPasswordChange(string userId)
    {
        var model = new NewPasswordVm { UserId = userId };
        return View("NewPasswordPage", model);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmNewPassword(NewPasswordVm model)
    {
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var passwordErrors = await _repository.ValidatePasswordAsync(model.NewPassword);
            if (passwordErrors.Any())
            {
                var description = string.Empty;
                foreach (var error in passwordErrors)
                {
                    description += string.Concat(error, "<br>");
                }
                ViewData["PasswordCheckError"] = description;
                return View("NewPasswordPage", model);
            }
        }
        if (!ModelState.IsValid)
        {
            return View("NewPasswordPage", model);
        }

        if (!await _repository.CheckIfUserExistsAsync(model.UserId))
        {
            TempData["UserNotFound"] = "User not found!";
            return View("NewPasswordPage", model);
        }

        if (await _repository.ConfirmNewPasswordAsync(model))
        {
            TempData["PasswordMessage"] = true;
            return RedirectToAction("AllProducts", "Product");
        }
        return View("NewPasswordPage", model);
    }
    [HttpPost]
    public async Task<IActionResult> SendEmailConfirmationLink(string email)
    {
        await _repository.SendEmailConfirmationLinkAsync(email);
        TempData["MessageSent"] = true;
        return RedirectToAction("MyProfile", "User");
    }

   
}
