using System.Security.Claims;

namespace RetailRally.Helpers;

public static class ClaimPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
