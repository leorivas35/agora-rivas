using Core.Constants;

namespace Core.Entities;

/// <summary>
/// Represents a file and its metadata, including its Title, content type,
/// storage URI, size in bytes, and document type. For information about Azure Blob Storage,
/// see <see href="https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-overview">the Azure Blob Storage overview</see>.
/// </summary>
public class AgoraFile : BaseEntity
{
    public string Title { get; set; } = default!;    
    public string ContentType { get; set; } = default!;
    public string Extension { get; set; } = default!;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Public URL or SAS URL to access the document.</summary>
    public string Uri { get; set; } = null!;

    /// <summary>Navigation property to the document content.</summary>
    public DocumentContent Content { get; set; } = default!; //Keep the file bytes in a separate table from the metadata so that listing documents doesn’t drag the binary data along.



}
