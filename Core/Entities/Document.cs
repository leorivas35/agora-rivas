using Core.Constants;

namespace Core.Entities;

/// <summary>
/// Represents a document and its metadata, including its title, author, content type,
/// storage URI, size in bytes, and document type. For information about Azure Blob Storage,
/// see <see href="https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-overview">the Azure Blob Storage overview</see>.
/// </summary>
public class Document : BaseEntity
{    
    public required string Title { get; set; }
    public string? Author { get; set; }    
    public required string ContentType { get; set; }   // "image/png", "application/pdf"
    /// <summary>Public URL or SAS URL to access the document.</summary>
    public string? Uri { get; set; } 
    public string? SizeInBytes { get; set; }  
    /// <summary>Enum representing the type of the document, such as a Resume, Cover Letter, or Book.</summary>
    public required DocumentType Type { get; set; } 

}
