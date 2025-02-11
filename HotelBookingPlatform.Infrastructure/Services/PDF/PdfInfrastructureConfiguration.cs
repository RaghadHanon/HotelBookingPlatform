using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace HotelBookingPlatform.Infrastructure.Services.PDF;

public static class PdfInfrastructureConfiguration
{
    public static IServiceCollection AddPdfInfrastructure(
        this IServiceCollection services)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return services.AddScoped<IPdfGenerator, PdfGenerator>();
    }
}
