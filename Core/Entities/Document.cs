using Core.Constants;

namespace Core.Entities;

public class Document : BaseEntity
{
    public required string FileName { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string ContentType { get; set; }   // "image/png", "application/pdf"
    public required long SizeInBytes { get; set; }
    public required string StoragePath { get; set; }   // or a blob URL
    /// <summary>
    /// Enum representing the type of the document. For example, it could indicate whether the document is a Resume, Cover Letter, Book, or any other specific type.
    /// </summary>
    public required DocumentType Type { get; set; } 

}
