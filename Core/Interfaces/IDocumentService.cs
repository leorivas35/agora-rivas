using Core.Entities;

namespace Core.Interfaces;

public interface IDocumentService
{    
    Task<Document> GetDocumentAsync(string id);
    Task<IEnumerable<Document>> GetAllDocumentsAsync();
    void CreateDocument(Document document);
    void UpdateDocument(Document document);
    void DeleteDocument(string id);
    Task<bool> SaveChangesAsync();

}
