using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RetailRally.Models;
using RetailRally.ViewModels;

namespace RetailRally.Interfaces;

public interface IAccountRepository
{
    Task<bool> CheckIfUsernameExistsAsync(string email);
    Task<bool> CheckIfEmailExistsAsync(string username);
    Task<IEnumerable<string>> ValidatePasswordAsync(string password);
    Task<User> FindUserbyEmailAsync(string email);
    Task<bool> CheckIfCorrectPasswordAsync(string password, User user);
    Task<bool> LogIn(User user, string password, bool rememberMe);
    Task<IdentityResult> CreateUser(User user, string password, IFormFile userImage = null, string pictureUrl = null);
    Task SignOutAsync();
    Task<bool> CheckEmailValidation(string email);
    Task<bool> CheckIfUserExistsAsync(string id);
    Task<bool> SendEmailChangeLinkAsync(string email);
    Task<bool> SendPasswordChangeLinkAsync(string email);
    Task<bool> SendEmailConfirmationLinkAsync(string email);
    Task<bool> ConfirmNewEmailAsync(NewEmailVm model);
    Task<bool> ConfirmNewPasswordAsync(NewPasswordVm model);
    Task<bool> ConfirmEmailAsync(string userId);
}
