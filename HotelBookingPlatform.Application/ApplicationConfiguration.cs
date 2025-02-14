using FluentValidation;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Application.Validators.Bookings;
using Microsoft.Extensions.DependencyInjection;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;

namespace HotelBookingPlatform.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
        => services
            .AddAutoMapper(Assembly.GetExecutingAssembly())
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddScoped<ICityService, CityService>()
            .AddScoped<IHotelService, HotelService>()
            .AddScoped<IRoomService, RoomService>()
            .AddScoped<IBookingService, BookingService>()
            .AddScoped<IInvoiceService, InvoiceService>()
            .AddScoped<IGuestService, GuestService>()
            .AddScoped<IReviewService, ReviewService>()
            .AddScoped<IDiscountService, DiscountService>()
            .AddTransient<IAuthenticationService, AuthenticationService>()
            .AddTransient<IAmenityService, AmenityService>()
            .AddScoped<BookingServiceValidator>();
}

