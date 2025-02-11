using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Validators.Discounts;

public class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
{
    public CreateDiscountDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow)
            .WithMessage(ValidationErrorMessages.StartDateMustBeFuture);

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage(ValidationErrorMessages.EndDateMustBeAfterStartDate);

        When(x => x.Percentage != null, () =>
        {
            RuleFor(x => x.Percentage)
                .InclusiveBetween(1, 100)
                .WithMessage(ValidationErrorMessages.PercentageRange);
        });

        When(x => x.DiscountedPrice != null, () =>
        {
            RuleFor(x => x.DiscountedPrice)
                .GreaterThan(0)
                .WithMessage(ValidationErrorMessages.DiscountedPriceGreaterThanZero);
        });

        RuleFor(x => new { x.Percentage, x.DiscountedPrice })
            .Must(x => x.Percentage != null || x.DiscountedPrice != null)
            .WithMessage(ValidationErrorMessages.MustSupplyPercentageOrDiscountedPrice);
    }
}
