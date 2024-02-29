namespace RetailRally.ViewModels;

public class ChatVm
{
    public string CurrentUserName { get; set; }

    public string OtherUserId { get; set; }
    public string OtherUsername { get; set; }
    public List<MessagesVm> Messages { get; set; } 
}
