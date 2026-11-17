using Core.Entities;
    
namespace Core.Interfaces;

public interface IFileService
{    
    Task<AgoraFile> GetFileAsync(string id);
    Task<IEnumerable<AgoraFile>> GetAllFilesAsync();
    Task<AgoraFile> CreateFileAsync(string title, string contentType, string extension, Stream content, CancellationToken cancellationToken = default);
    void UpdateFile(AgoraFile file);
    void DeleteFile(string id);
    Task<bool> SaveChangesAsync();

}
