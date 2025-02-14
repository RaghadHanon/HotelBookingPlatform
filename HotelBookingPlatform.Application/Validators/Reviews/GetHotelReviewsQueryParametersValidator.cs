using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Reviews;

public class GetHotelReviewsQueryParametersValidator : AbstractValidator<GetHotelReviewsQueryParameters>
{
    public GetHotelReviewsQueryParametersValidator()
    {
        Include(new QueryParametersValidator<ReviewSortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidReviewSortColumn);
    }
}
