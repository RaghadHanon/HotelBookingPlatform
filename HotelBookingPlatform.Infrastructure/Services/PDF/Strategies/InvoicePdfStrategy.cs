using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Globalization;

namespace HotelBookingPlatform.Infrastructure.Services.PDF.Strategies;

public class InvoicePdfStrategy : IPdfGenerationStrategy
{
    [Obsolete]
    public byte[] GeneratePdf(InvoiceDto invoice)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.Letter);

                // Header
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Hotel Booking Platform")
                                .FontSize(16)
                                .Bold()
                                .FontColor(Colors.Indigo.Darken3);
                            col.Item().Text("Palestine");
                            col.Item().Text("raghadhanoon2015@gmail.com");
                        });
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("Booking Invoice")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Indigo.Darken3);
                            col.Item().Text($"Date: {DateTime.Now:MMMM dd, yyyy}");
                            col.Item().Text($"Confirmation ID: {invoice.ConfirmationId}");
                        });
                    });
                    header.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                // Content
                page.Content().Column(content =>
                {
                    content.Spacing(10);
                    // Guest Information
                    content.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(col =>
                    {
                        col.Item().Text("Guest Information").FontSize(16).Bold().FontColor(Colors.Indigo.Darken3);
                        col.Item().Text(invoice.GuestFullName).FontSize(14);
                    });
                    // Hotel Information
                    content.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(col =>
                    {
                        col.Item().Text("Hotel Information").FontSize(16).Bold().FontColor(Colors.Indigo.Darken3);
                        col.Item().Text($"{invoice.Hotel.Name}, {invoice.Hotel.CityName}").FontSize(14);
                        col.Item().Text(invoice.Hotel.Street);
                    });
                    // Booking Dates
                    content.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(col =>
                    {
                        col.Item().Text("Booking Information").FontSize(16).Bold().FontColor(Colors.Indigo.Darken3);
                        col.Item().Text($"Check-In: {invoice.CheckInDate:MMMM dd, yyyy}");
                        col.Item().Text($"Check-Out: {invoice.CheckOutDate:MMMM dd, yyyy}");
                        col.Item().Text($"numberOfAdults: {invoice.NumberOfAdults}");
                        col.Item().Text($"numberOfChildren: {invoice.NumberOfChildren}");
                    });
                    // Room Details
                    content.Item().Column(col =>
                    {
                        col.Item().Text("Room Details").FontSize(16).Bold().FontColor(Colors.Indigo.Darken3);
                        col.Item().Grid(grid =>
                        {
                            grid.Columns(6);
                            grid.Spacing(5);
                            grid.VerticalSpacing(5);
                            // Table Header
                            grid.Item().Background(Colors.Indigo.Medium).Padding(5).Text("Room #").FontColor(Colors.White).FontSize(12).Bold();
                            grid.Item().Background(Colors.Indigo.Medium).Padding(5).Text("Type").FontColor(Colors.White).FontSize(12).Bold();
                            grid.Item().Background(Colors.Indigo.Medium).Padding(5).Text("Capacity").FontColor(Colors.White).FontSize(12).Bold();
                            grid.Item().Background(Colors.Indigo.Medium).Padding(5).Text("Price/Night").FontColor(Colors.White).FontSize(12).Bold();
                            grid.Item().Background(Colors.Indigo.Medium).Padding(5).Text("Total").FontColor(Colors.White).FontSize(12).Bold();
                            grid.Item().Background(Colors.Indigo.Medium).Padding(5).Text("After Discount").FontColor(Colors.White).FontSize(12).Bold();
                            // Table Rows
                            foreach (Application.DTOs.Room.RoomWithinInvoiceDto room in invoice.Rooms)
                            {
                                grid.Item().Background(Colors.Grey.Lighten5).Padding(5).Text(room.RoomNumber.ToString()).FontSize(11);
                                grid.Item().Background(Colors.Grey.Lighten5).Padding(5).Text(room.RoomType).FontSize(11);
                                grid.Item().Background(Colors.Grey.Lighten5).Padding(5).Text($"{room.AdultsCapacity} Adults, {room.ChildrenCapacity} Children").FontSize(11);
                                grid.Item().Background(Colors.Grey.Lighten5).Padding(5).Text(room.PricePerNight.ToString("C")).FontSize(11);
                                grid.Item().Background(Colors.Grey.Lighten5).Padding(5).Text(room.TotalRoomPrice.ToString("C")).FontSize(11);
                                grid.Item().Background(Colors.Grey.Lighten5).Padding(5).Text(room.TotalRoomPriceAfterDiscount.ToString("C")).FontColor(Colors.Green.Darken1).FontSize(11);
                            }
                        });
                    });
                    // Totals
                    content.Item().PaddingTop(10).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Total Price:").Bold().FontSize(14);
                            row.RelativeItem().AlignRight().Text(invoice.TotalPrice.ToString("C")).FontSize(14).Bold();
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Total Price After Discount:").FontSize(14).Bold().FontColor(Colors.Green.Darken1);
                            row.RelativeItem().AlignRight().Text(invoice.TotalPriceAfterDiscount.ToString("C")).FontSize(14).Bold().FontColor(Colors.Green.Darken1);
                        });
                    });
                    // Add provisional watermark as part of content
                    content.Item().Element(container =>
                    {
                        container
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Hotel Booking Blatform")
                            .FontSize(50)
                            .FontColor(Colors.Grey.Lighten3);
                    });

                });

                // Footer
                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    footer.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Thank you for choosing Hotel Booking Platform! ");
                            t.Span("This is a provisional invoice.").FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span($"Page ").FontColor(Colors.Grey.Medium);
                            text.CurrentPageNumber().FontColor(Colors.Indigo.Darken3);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
