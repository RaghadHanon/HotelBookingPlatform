using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using HotelBookingPlatform.Infrastructure.Interfaces.Factories;
using HotelBookingPlatform.Infrastructure.Services.PDF.Strategies;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace HotelBookingPlatform.Infrastructure.Services.PDF;

public static class PdfInfrastructureConfiguration
{
    public static IServiceCollection AddPdfInfrastructure(
        this IServiceCollection services)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        services.AddScoped<IPdfGenerator, PdfGenerator>();
        services.AddTransient<InvoicePdfStrategy>();
        services.AddSingleton<IPdfStrategyFactory, PdfStrategyFactory>();

        return services;
    }
}
