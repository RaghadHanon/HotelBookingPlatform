using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Validators.Common;

public class QueryParametersValidator<TSortColumn> : AbstractValidator<QueryParameters<TSortColumn>> where TSortColumn : Enum
{
    public QueryParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationErrorMessages.PageNumberInvalid);

        RuleFor(x => x.PageSize)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationErrorMessages.PageSizeInvalid);

        RuleFor(x => x.SortOrder)
            .Must(x => x.HasValue || x == SortOrder.Asc || x == SortOrder.Desc)
            .WithMessage(ValidationErrorMessages.InvalidSortOrder);

        RuleFor(x => x.SearchTerm)
            .Must(x => string.IsNullOrEmpty(x) || x.Length <= 100)
            .WithMessage(ValidationErrorMessages.SearchTermTooLong);
    }
}
