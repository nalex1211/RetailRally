using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using RetailRally.Models;
using RetailRally.ViewModels;
using System.Text;

namespace RetailRally.Helpers;

public class AzureStorageService(IConfiguration _configuration)
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

    //public async Task UploadMessageAsync(string containerName, string senderUsername, string receiverUsername, string message)
    //{
    //    string blobName = $"{senderUsername}_to_{receiverUsername}.txt";

    //    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
    //    await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

    //    var blobClient = containerClient.GetBlobClient(blobName);

    //    if (await blobClient.ExistsAsync())
    //    {
    //        var appendBlobClient = containerClient.GetAppendBlobClient(blobName);
    //        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(message + Environment.NewLine));
    //        await appendBlobClient.AppendBlockAsync(memoryStream);
    //    }
    //    else
    //    {
    //        var appendBlobClient = containerClient.GetAppendBlobClient(blobName);
    //        await appendBlobClient.CreateAsync();
    //        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(message + Environment.NewLine));
    //        await appendBlobClient.AppendBlockAsync(memoryStream);
    //    }
    //}
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


    //public async Task<List<MessagesVm>> GetMessagesAsync(string containerName, string currentUsername, string otherUsername)
    //{
    //    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
    //    var currentToOtherBlob = containerClient.GetBlobClient($"{currentUsername}_to_{otherUsername}.txt");
    //    var otherToCurrentBlob = containerClient.GetBlobClient($"{otherUsername}_to_{currentUsername}.txt");

    //    var messages = new List<MessagesVm>();

    //    if (await currentToOtherBlob.ExistsAsync())
    //    {
    //        var response = await currentToOtherBlob.DownloadAsync();
    //        using var reader = new StreamReader(response.Value.Content);
    //        while (!reader.EndOfStream)
    //        {
    //            var messageContent = await reader.ReadLineAsync();
    //            var fullMessageContent = $"{currentUsername}| {messageContent}";
    //            messages.Add(new MessagesVm(fullMessageContent));
    //        }
    //    }

    //    if (await otherToCurrentBlob.ExistsAsync())
    //    {
    //        var response = await otherToCurrentBlob.DownloadAsync();
    //        using var reader = new StreamReader(response.Value.Content);
    //        while (!reader.EndOfStream)
    //        {
    //            var messageContent = await reader.ReadLineAsync();
    //            var fullMessageContent = $"{otherUsername}| {messageContent}";
    //            messages.Add(new MessagesVm(fullMessageContent));
    //        }
    //    }


    //    return messages;
    //}

}
