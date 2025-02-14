using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Factories;

public interface IPdfStrategyFactory
{
    IPdfGenerationStrategy CreateStrategy(PdfType pdfType);
}
