namespace API.Middleware;

using API.Errors;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

public class ExceptionMiddleware(IHostEnvironment env, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Handle the exception (e.g., log it, return a custom error response)            
            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex, IHostEnvironment env)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        ApiErrorResponse response = env.IsDevelopment()
            ? new ApiErrorResponse(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
            : new ApiErrorResponse(context.Response.StatusCode, "An unexpected error occurred.");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        string jsonResponse = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(jsonResponse);
        
    }
}