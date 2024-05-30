using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Interfaces;
using RetailRally.ViewModels;

namespace RetailRally.Helpers;

public class UserProfileService(HubContextClass _db) : IUserProfileService
{
    public async Task<UserProfileViewModel> GetUserProfileAsync(string userId)
    {
        var user = await _db.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }

        return new UserProfileViewModel
        {
            PictureUrl = user.PictureUrl,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }
    public async Task<string> GetUserPictureAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return string.Empty;
        }
        return user.PictureUrl;
    }
}
