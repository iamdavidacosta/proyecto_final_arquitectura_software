using FileIngestionService.Application.Interfaces;
using FileIngestionService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileIngestionService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFileIngestionService, FileIngestionServiceImpl>();
        return services;
    }
}
