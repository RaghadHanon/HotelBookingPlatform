using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Discounts;

public class GetDiscountsQueryParametersValidator : AbstractValidator<GetDiscountsQueryParametersDto>
{
    public GetDiscountsQueryParametersValidator()
    {
        Include(new QueryParametersValidator<DiscountSortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidDiscountSortColumn);
    }
}
