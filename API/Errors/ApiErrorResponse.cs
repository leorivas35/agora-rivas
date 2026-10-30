namespace API.Errors;
/// <summary>
/// Represents a standardized API error response.
/// </summary>
/// <param name="statusCode">The HTTP status code of the error.</param>
/// <param name="message">A brief message describing the error.</param>
/// <param name="details">Optional detailed information about the error.</param>
public class ApiErrorResponse(int statusCode, string message, string? details = null)
{
    public int StatusCode { get; } = statusCode;
    public string Message { get; } = message;
    public string? Details { get; } = details;
}