using Microsoft.AspNetCore.Mvc;
using RetailRally.Interfaces;
using System.Security.Claims;

namespace RetailRally.Helpers;

public class UserProfileViewComponent(IUserProfileService _userProfileService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string viewName)
    {
        var userId = UserClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userProfile = await _userProfileService.GetUserProfileAsync(userId);

        if (viewName == "UserProfileEmailOnly")
        {
            return View("UserProfileEmailOnly", userProfile);
        }
        else
        {
            return View("UserProfile", userProfile);
        }
    }
}
