using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Identity;
using RetailRally.Models;
using RetailRally.ViewModels;
using System.Text;

namespace RetailRally.Helpers;

public class AzureStorageService(IConfiguration _configuration, UserManager<User> _userManager)
{
    private readonly BlobServiceClient _blobServiceClient = new BlobServiceClient(_configuration["AzureStorageConfig:ConnectionString"]);

    public async Task<string> UploadImageAsync(Stream imageStream, string imageName, string containerName)
    {
        string fileExtension = Path.GetExtension(imageName);
        string uniqueFileName = $"{imageName}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(uniqueFileName);
        await blobClient.UploadAsync(imageStream, new BlobHttpHeaders { ContentType = "image/jpeg" });
        return blobClient.Uri.ToString();
    }
    public async Task DeleteImageAsync(string imageUrl, string containerName)
    {
        var uri = new Uri(imageUrl);
        string fileName = Path.GetFileName(uri.LocalPath);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }

    public async Task UploadMessageAsync(string containerName, string senderUsername, string receiverUsername, string message)
    {
        string blobName = $"{senderUsername}_to_{receiverUsername}.txt";

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobClient = containerClient.GetBlobClient(blobName);
        string messageWithTimestamp = $"{DateTime.UtcNow:o}|{message}{Environment.NewLine}";

        if (await blobClient.ExistsAsync())
        {
            var appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(messageWithTimestamp));
            await appendBlobClient.AppendBlockAsync(memoryStream);
        }
        else
        {
            var appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            await appendBlobClient.CreateAsync();
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(messageWithTimestamp));
            await appendBlobClient.AppendBlockAsync(memoryStream);
        }
    }

    public async Task<List<MessagesVm>> GetMessagesAsync(string containerName, string currentUsername, string otherUsername)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobs = new[] { $"{currentUsername}_to_{otherUsername}", $"{otherUsername}_to_{currentUsername}" };

        var messages = new List<MessagesVm>();

        foreach (var blobPrefix in blobs)
        {
            var blobClient = containerClient.GetBlobClient(blobPrefix + ".txt");
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                using var reader = new StreamReader(response.Value.Content);
                while (!reader.EndOfStream)
                {
                    var messageLine = await reader.ReadLineAsync();
                    var messageParts = messageLine.Split('|');
                    if (messageParts.Length >= 2)
                    {
                        DateTime timestamp = DateTime.Parse(messageParts[0]);
                        string text = messageParts[1];
                        string senderUsername = blobPrefix.StartsWith($"{currentUsername}_to_") ? currentUsername : otherUsername;
                        string receiverUsername = senderUsername == currentUsername ? otherUsername : currentUsername;
                        messages.Add(new MessagesVm
                        {
                            SenderUsername = senderUsername,
                            ReceiverUsername = receiverUsername,
                            Text = text,
                            Timestamp = timestamp
                        });
                    }
                }
            }
        }

        messages.Sort((x, y) => DateTime.Compare(x.Timestamp, y.Timestamp));

        return messages;
    }

    //public async Task<List<ChatSummaryVm>> GetChatListAsync(string currentUsername, string containerName)
    //{
    //    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
    //    var chatSummaries = new List<ChatSummaryVm>();

    //    await foreach (var blobItem in containerClient.GetBlobsAsync())
    //    {
    //        var blobNameParts = blobItem.Name.Replace(".txt", "").Split("_to_");
    //        if (blobNameParts.Length == 2)
    //        {
    //            var sender = blobNameParts[0];
    //            var receiver = blobNameParts[1];

    //            if (sender == currentUsername || receiver == currentUsername)
    //            {
    //                var otherUser = sender == currentUsername ? receiver : sender;
    //                var otherUser1 = await _userManager.FindByNameAsync(otherUser);

    //                chatSummaries.Add(new ChatSummaryVm
    //                {
    //                    OtherUsername = otherUser,
    //                    OtherUserId = otherUser1.Id
    //                });
    //            }
    //        }
    //    }

    //    return chatSummaries;
    //}

    public async Task<List<ChatSummaryVm>> GetChatListAsync(string currentUsername, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var chatSummaries = new List<ChatSummaryVm>();
        var addedUsers = new HashSet<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            var blobNameParts = blobItem.Name.Replace(".txt", "").Split("_to_");
            if (blobNameParts.Length == 2)
            {
                var sender = blobNameParts[0];
                var receiver = blobNameParts[1];

                if (sender == currentUsername || receiver == currentUsername)
                {
                    var otherUser = sender == currentUsername ? receiver : sender;
                    if (!addedUsers.Contains(otherUser))
                    {
                        var otherUser1 = await _userManager.FindByNameAsync(otherUser);

                        chatSummaries.Add(new ChatSummaryVm
                        {
                            OtherUsername = otherUser,
                            OtherUserId = otherUser1.Id
                        });

                        addedUsers.Add(otherUser);
                    }
                }
            }
        }
        return chatSummaries;
    }

}
