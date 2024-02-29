namespace RetailRally.ViewModels;

public class MessagesVm
{
    public string SenderUsername { get; set; }
    public string ReceiverUsername { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp {  get; set; }
    public override string ToString()
    {
        // You can customize the format as needed
        return $"{Timestamp:g} - {SenderUsername} says: {Text}";
    }

    //public MessagesVm(string blobContent)
    //{
    //    var contentParts = blobContent.Split('|');
    //    if (contentParts.Length >= 4)
    //    {
    //        SenderUsername = contentParts[0].Trim();
    //        ReceiverUsername = contentParts[1].Trim();
    //        Text = contentParts[2].Trim();
    //        Timestamp = DateTime.Parse(contentParts[3].Trim());
    //    }
    //}

    //public override string ToString()
    //{
    //    return $"{SenderUsername}:{Text}";
    //}

    //public string ToBlobString()
    //{
    //    return $"{SenderUsername}|{ReceiverUsername}|{Text}";
    //}
}

