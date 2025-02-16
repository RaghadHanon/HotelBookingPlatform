using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Infrastructure.Interfaces.Factories;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;
using HotelBookingPlatform.Infrastructure.Services.PDF.Strategies;
using HotelBookingPlatform.Infrastructure.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingPlatform.Infrastructure.Services.PDF;
public class PdfStrategyFactory : IPdfStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PdfStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPdfGenerationStrategy CreateStrategy(PdfType pdfType)
    {
        return pdfType switch
        {
            PdfType.InvoicePdf => _serviceProvider.GetRequiredService<InvoicePdfStrategy>(),
            _ => throw new ArgumentException(InfrastructureErrorMessages.NoPdfGeneratorStrategyFound, pdfType.ToString())
        };
    }
}