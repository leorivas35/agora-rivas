using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => 
    { 
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    });

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddCors();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AgoraRivasContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));    
           
});
builder.Services.AddIdentityApiEndpoints<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AgoraRivasContext>();
#endregion

WebApplication app = builder.Build();

#region Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4003", "https://localhost:4003"));

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();
app.Run();
#endregion


// builder.Services.AddRateLimiter(options =>
// {
//     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//     options.AddPolicy(ApiConst.RateLimitPolicy, context =>
//         RateLimitPartition.GetFixedWindowLimiter(
//             context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
//             _ => new FixedWindowRateLimiterOptions
//             {
//                 PermitLimit = 20,
//                 Window = TimeSpan.FromMinutes(1),
//                 QueueLimit = 0
//             }));
// });