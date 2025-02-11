using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Infrastructure.Persistence.Repositories;
using HotelBookingPlatform.Infrastructure.Services.Image;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingPlatform.Infrastructure.Persistence;

public static class PersistenceConfiguration
{
    public static IServiceCollection AddPersistenceInfrastructure
        (this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        => services
            .AddDatabase(GetConnectionString(configuration, isDevelopment), isDevelopment)
            .AddRepositories()
            .AddImageHandling();

    private static string GetConnectionString(IConfiguration configuration, bool isDevelopment)
    {
        return configuration.GetConnectionString("SqlServer");
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services, string connectionString, bool isDevelopment)
        => services
            .AddDbContext<HotelBookingPlatformDbContext>(opt =>
                opt.UseSqlServer(connectionString)
                   .EnableSensitiveDataLogging(isDevelopment)
                   .EnableDetailedErrors(isDevelopment));

    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
        => services
            .AddScoped<ICityRepository, CityRepository>()
            .AddScoped<IHotelRepository, HotelRepository>()
            .AddScoped<IRoomRepository, RoomRepository>()
            .AddScoped<IGuestRepository, GuestRepository>()
            .AddScoped<IBookingRepository, BookingRepository>()
            .AddScoped<IGuestRepository, GuestRepository>()
            .AddScoped<IReviewRepository, ReviewRepository>()
            .AddScoped<IDiscountRepository, DiscountRepository>()
            .AddScoped<IAmenityRepository, AmenityRepository>();

    public static IServiceCollection AddImageHandling(
        this IServiceCollection services)
        => services.AddScoped<IImageHandler, ImageHandler>();

    public static IApplicationBuilder Migrate(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using HotelBookingPlatformDbContext context = scope.ServiceProvider.GetRequiredService<HotelBookingPlatformDbContext>();
        context.Database.Migrate();
        return app;
    }
}
