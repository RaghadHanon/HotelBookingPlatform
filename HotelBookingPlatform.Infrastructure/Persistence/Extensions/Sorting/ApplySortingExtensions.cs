using HotelBookingPlatform.Application.Enums;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class ApplySortingExtensions
{
    public static IQueryable<T> ApplySorting<T>
        (this IQueryable<T> query, SortOrder? sortOrder, Expression<Func<T, object>> keySelector)
    {
        if (sortOrder == SortOrder.Desc)
        {
            query = query.OrderByDescending(keySelector);
        }
        else
        {
            query = query.OrderBy(keySelector);
        }

        return query;
    }
}
