using Microsoft.AspNetCore.SignalR;

namespace RetailRally.Helpers;

public class ChatHub(AzureStorageService _service, IConfiguration _configuration) : Hub
{
    private static readonly Dictionary<string, string> UserConnections = new Dictionary<string, string>();
    private readonly string _containerName = _configuration["AzureStorageConfig:MessagesContainer"];
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.GetUserId();
        UserConnections[userId] = Context.ConnectionId;
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User?.GetUserId();
        UserConnections.Remove(userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToUser(string senderUsername, string receiverId, string otherUsername, string message)
    {
        if (UserConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderUsername, message);
        }

        await Clients.Caller.SendAsync("ReceiveMessage", senderUsername, message);

        await _service.UploadMessageAsync(_containerName, senderUsername, otherUsername, message);
    }
}
