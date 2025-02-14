using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Handles booking-related operations such as retrieving, creating, and deleting bookings.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.GuestOnly)]
[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Get a booking by its ID
    /// </summary>
    /// <param name="id">The ID of the booking</param>
    /// <returns>The booking with the given ID</returns>
    /// <response code="200">Returns the booking with the given ID</response>
    /// <response code="404">When the booking is not found</response>
    [HttpGet("{id}", Name = "GetBooking")]
    public async Task<ActionResult<BookingOutputDto>> GetBooking(Guid id)
    {
        _logger.LogInformation(APILogMessages.GetBookingStarted, id);
        BookingOutputDto? booking = await _bookingService.GetBookingAsync(id);
        _logger.LogInformation(APILogMessages.GetBookingCompleted, id);

        return Ok(booking);
    }

    /// <summary>
    /// Get an invoice by booking ID
    /// </summary>
    /// <param name="id">The booking ID</param>
    /// <returns>The invoice with the given ID</returns>
    /// <response code="200">Returns the invoice with the given ID</response>
    /// <response code="404">If there is no booking related to the given ID</response>
    [HttpGet("{id}/invoice")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        _logger.LogInformation(APILogMessages.GetInvoiceStarted, id);
        InvoiceDto? invoice = await _bookingService.GetInvoiceAsync(id);
        _logger.LogInformation(APILogMessages.GetInvoiceCompleted, id);

        return Ok(invoice);
    }

    /// <summary>
    /// Create a new booking, and receive the invoice by email
    /// </summary>
    /// <param name="request">The data for the new booking</param>
    /// <returns>The newly created booking</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /booking
    ///     {
    ///        "roomsId": ["65d26773-a8b9-4695-bc61-0b717bd97e14", "98726773-a8b9-4695-bc61-0b717bd97e14"],
    ///        "hotelId": "87c26773-a8b9-4695-bc61-0b717bd97e14",
    ///        "checkInDate": "2025-1-02",
    ///        "checkOutDate": "2025-01-27",
    ///        "numberOfAdults": 2,
    ///        "numberOfChildren": 1,
    ///        "userRemarks": "Please make sure there’s a mini-fridge in the room.",
    ///        "paymentMethod": "Credit Card"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created booking</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    /// <response code="404">If the hotel/room or guest is not found</response>
    [HttpPost]
    public async Task<ActionResult<BookingOutputDto>> CreateBooking(CreateBookingDto request)
    {
        _logger.LogInformation(APILogMessages.CreateBookingStarted, request);
        BookingOutputDto booking = await _bookingService.CreateBookingAsync(request);
        _logger.LogInformation(APILogMessages.CreateBookingCompleted, request);

        return CreatedAtAction(nameof(GetBooking), new { id = booking.ConfirmationId }, booking);
    }

    /// <summary>
    /// Delete a booking
    /// </summary>
    /// <param name="id">The ID of the booking to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the operation is successfully done</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        _logger.LogInformation(APILogMessages.DeleteBookingStarted, id);
        await _bookingService.DeleteBookingAsync(id);
        _logger.LogInformation(APILogMessages.DeleteBookingCompleted, id);

        return NoContent();
    }

    /// <summary>
    /// Retrieve a booking invoice as a PDF file to download it
    /// </summary>
    /// <returns> The invoice as pdf file </returns>
    /// <response code="200">Returns the invoice with the given ID</response>
    /// <response code="404">If the hotel/room or guest is not found</response>
    [HttpGet("{id}/pdf")]
    public async Task<FileContentResult> GetInvoicePdf(Guid id)
    {
        byte[] pdfBytes = await _bookingService.GetInvoicePdfByBookingIdAsync(id);

        return File(pdfBytes, "application/pdf", "invoice.pdf");
    }
}
