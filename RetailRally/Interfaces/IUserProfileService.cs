using RetailRally.ViewModels;

namespace RetailRally.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileViewModel> GetUserProfileAsync(string userId);
    Task<string> GetUserPictureAsync(string userId);
}
