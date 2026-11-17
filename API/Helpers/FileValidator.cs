using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers;

public static class FileValidator
{
    // Summary: maps supported file extensions to the byte signatures used to identify their file formats.
    private static readonly Dictionary<string, byte[]> signatures = new()
    {
        [".pdf"] = "%PDF"u8.ToArray(),  // UTF-8 string literal - matches the PDF file signature. This code is same as new byte[] { 0x25, 0x50, 0x44, 0x46 }
        [".docx"] = new byte[] { 0x50, 0x4B, 0x03, 0x04 },  // DOCX files are ZIP containers and typically start with this signature.
    };
    // Summary: maps supported file extensions to the MIME content types used to identify their file formats.
    private static readonly Dictionary<string, string[]> contentTypes = new()
    {
        [".pdf"] = ["application/pdf"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
    };

    /// <summary>
    /// Validates that an uploaded file is non-empty, has a supported extension and matching content type,
    /// and begins with the expected file signature.
    /// </summary>
    /// <returns>Whether the file is valid, an error message if invalid, and its extension if valid.</returns>
    public static async Task<(bool isValid, string? errorMessage, string? fileExtension)> ValidateAsync(IFormFile file)
    {
        if(file.Length == 0)
        {
            return (false, "File is empty.", null);
        }

        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!signatures.ContainsKey(fileExtension))
        {
            return (false, "Only .pdf and .docx files are allowed.", null);
        }
        if(!contentTypes[fileExtension].Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return (false, "Content type does not match the file extension.", null);
        }

        byte[] expectedSignature = signatures[fileExtension];
        byte[] header = new byte[expectedSignature.Length];

        await using Stream stream = file.OpenReadStream();
        int bytesRead = await stream.ReadAsync(header);

        if (bytesRead != expectedSignature.Length || !header.SequenceEqual(expectedSignature))
        {
            return (false, "File signature does not match the file extension.", null);
        }

        return (true, null, fileExtension);
    }

}