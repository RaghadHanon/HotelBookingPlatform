using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using HotelBookingPlatform.Infrastructure.Interfaces.Factories;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;

namespace HotelBookingPlatform.Infrastructure.Services.PDF;

public class PdfGenerator : IPdfGenerator
{
    private readonly IPdfStrategyFactory _pdfStrategyFactory;

    public PdfGenerator(IPdfStrategyFactory pdfStrategyFactory)
    {
        _pdfStrategyFactory = pdfStrategyFactory;
    }

    public byte[] GeneratePdf(InvoiceDto invoice, PdfType pdfType)
    {
        IPdfGenerationStrategy strategy = _pdfStrategyFactory.CreateStrategy(pdfType);

        return strategy.GeneratePdf(invoice);
    }
}
