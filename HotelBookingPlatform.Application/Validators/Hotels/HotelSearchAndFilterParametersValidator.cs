using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Hotels;

public class HotelSearchAndFilterParametersValidator : AbstractValidator<HotelSearchAndFilterParametersDto>
{
    public HotelSearchAndFilterParametersValidator()
    {
        Include(new QueryParametersValidator<HotelSearchSortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidFilterHotelSortColumn);

        RuleFor(x => x.CheckInDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage(ValidationErrorMessages.CheckInDateInvalid);

        RuleFor(x => x.CheckOutDate)
            .GreaterThanOrEqualTo(x => x.CheckInDate)
            .WithMessage(ValidationErrorMessages.CheckOutDateInvalid);

        RuleFor(x => x.Adults)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationErrorMessages.AdultsInvalid);

        RuleFor(x => x.Children)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationErrorMessages.ChildrenInvalid);

        RuleFor(x => x.Rooms)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationErrorMessages.RoomsInvalid);

        When(x => x.MinStarRating != null, () =>
        {
            RuleFor(x => x.MinStarRating)
                .InclusiveBetween(1, 5)
                .WithMessage(ValidationErrorMessages.MinStarRatingInvalid);
        });

        When(x => x.MinPrice != null, () =>
        {
            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationErrorMessages.MinPriceInvalid);
        });

        When(x => x.MaxPrice != null, () =>
        {
            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationErrorMessages.MaxPriceInvalid)
                .GreaterThanOrEqualTo(x => x.MinPrice ?? 0)
                .WithMessage(ValidationErrorMessages.MaxPriceLessThanMinPrice);
        });

        When(x => x.RoomTypes != null, () =>
        {
            RuleForEach(x => x.RoomTypes)
                .IsInEnum()
                .WithMessage(ValidationErrorMessages.RoomTypeInvalid);
        });
    }
}
