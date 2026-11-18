using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Interfaces;

namespace Infrastructure.Services;

public sealed class AzureBlobDocumentStorage(Lazy<BlobContainerClient> containerClient) : IDocumentBlobStorage
{
    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        string extension = Path.GetExtension(fileName.Replace('\\', '/'));
        string blobName = $"{Guid.NewGuid():N}{extension}";
        BlobContainerClient container = containerClient.Value;
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        BlobClient blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            },
            cancellationToken);

        return blob.Uri.AbsoluteUri;
    }
}
