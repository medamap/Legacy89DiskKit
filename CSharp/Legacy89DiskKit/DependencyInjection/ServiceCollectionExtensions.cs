using Microsoft.Extensions.DependencyInjection;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Infrastructure.Factory;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Factory;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Factory;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.CharacterEncoding.Application;

namespace Legacy89DiskKit.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLegacy89DiskKit(this IServiceCollection services)
    {
        // Factory services
        services.AddSingleton<IDiskContainerFactory, DiskContainerFactory>();
        services.AddSingleton<IFileSystemFactory, FileSystemFactory>();
        services.AddSingleton<ICharacterEncoderFactory, CharacterEncoderFactory>();
        
        // Application services
        services.AddTransient<DiskImageService>();
        services.AddTransient<FileSystemService>();
        services.AddTransient<CharacterEncodingService>();
        
        return services;
    }
}