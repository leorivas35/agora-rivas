using Azure.Storage.Blobs;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {        

        services.AddScoped<IFileService, FileService>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // services.AddScoped<IDocumentBlobStorage, AzureBlobDocumentStorage>(); It will be added when Azure Blob Storage implementation is needed

        return services;
    }
}
