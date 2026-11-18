using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IUnitOfWork unitOfWork;
    // private readonly IDocumentBlobStorage blobStorage; Implementation will be added when Azure Blob Storage is needed
    public FileService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
    public async Task<AgoraFile> CreateFileAsync(string title, string contentType, string extension, Stream content, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);

        var _file = new AgoraFile
        {
            Title = title,
            ContentType = contentType,
            Extension = extension,
            SizeInBytes = memoryStream.Length,
            CreatedAtUtc = DateTime.UtcNow,
            Content = new DocumentContent { Content = memoryStream.ToArray() },
            // Uri = await blobStorage.UploadAsync(memoryStream, file.Title, file.ContentType, cancellationToken)
            Uri = string.Empty // Will be set when Azure Blob Storage is implemented 
        };

        unitOfWork.Repository<AgoraFile>().Add(_file);
        return !await unitOfWork.Complete() ? throw new InvalidOperationException("The document metadata could not be saved.") : _file;
    }

    public void DeleteFile(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AgoraFile>> GetAllFilesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<AgoraFile> GetFileAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }

    public void UpdateFile(AgoraFile file)
    {
        throw new NotImplementedException();
    }
}