using HotelBookingPlatform.Application;
using HotelBookingPlatform.Infrastructure.Identity;
using HotelBookingPlatform.Infrastructure.Persistence;
using HotelBookingPlatform.Infrastructure.Services.Email;
using HotelBookingPlatform.Infrastructure.Services.PDF;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HotelBookingPlatform.API;
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });

        bool isDevelopment = builder.Environment.IsDevelopment();
        builder.Services.AddApplication();
        builder.Services.AddPersistenceInfrastructure(builder.Configuration, isDevelopment);
        builder.Services.AddIdentityInfrastructure(builder.Configuration);
        builder.Services.AddPdfInfrastructure();
        builder.Services.AddEmailInfrastructure(builder.Configuration);
        builder.Services.AddWebComponents(builder.Configuration);
        WebApplication app = builder.Build();
        app.UseStatusCodePages();
        app.UseExceptionHandler();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.Migrate();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
