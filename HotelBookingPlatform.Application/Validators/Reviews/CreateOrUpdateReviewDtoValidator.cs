using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using static HotelBookingPlatform.Domain.Restrictions.Review;

namespace HotelBookingPlatform.Application.Validators.Reviews;

public class CreateOrUpdateReviewDtoValidator : AbstractValidator<CreateOrUpdateReviewDto>
{
    public CreateOrUpdateReviewDtoValidator()
    {
        RuleFor(x => x.Rating)
            .NotEmpty()
            .InclusiveBetween(MinRating, MaxRating)
            .WithMessage(ValidationErrorMessages.RatingInvalid);

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .Length(MinReviewTitleLength, MaxReviewTitleLength)
                .WithMessage(ValidationErrorMessages.TitleInvalidLength);
        });

        RuleFor(x => x.Description)
            .NotEmpty()
            .Length(MinReviewDescriptionLength, MaxReviewDescriptionLength)
            .WithMessage(ValidationErrorMessages.DescriptionInvalidLength);
    }
}
