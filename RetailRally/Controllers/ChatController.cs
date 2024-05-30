using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RetailRally.Contexts;
using RetailRally.Helpers;
using RetailRally.Models;
using RetailRally.ViewModels;

namespace RetailRally.Controllers;
public class ChatController(HubContextClass hub, AzureStorageService _service, IConfiguration _configuration, UserManager<User> _userManager) : Controller
{
    private readonly string _containerName = _configuration["AzureStorageConfig:MessagesContainer"];
    public IActionResult Index()
    {
        return View(hub.Users.ToList());
    }

    public async Task<IActionResult> GoToMyChats()
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return View("_LoginPartial");
        }
        var user = await _userManager.FindByIdAsync(currentUserId);
        var chats = await _service.GetChatListAsync(user.UserName, _containerName);
        return View("MyChats",chats);
    }


    public async Task<IActionResult> Room(string userId)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
        {
            return View("_LoginPartial");
        }
        var currentUser = hub.Users.FirstOrDefault(x => x.Id == currentUserId);
        var otherUser = hub.Users.FirstOrDefault(x => x.Id == userId);

        var messages = await _service.GetMessagesAsync(_containerName, currentUser.UserName, otherUser.UserName);

        var model = new ChatVm
        {
            CurrentUserName = currentUser.UserName,
            OtherUserId = userId,
            OtherUsername = otherUser.UserName,
            Messages = messages  
        };

        return View(model);
    }

}
