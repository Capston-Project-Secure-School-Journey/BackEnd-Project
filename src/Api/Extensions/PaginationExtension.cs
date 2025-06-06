using MongoDB.Driver;

namespace Api.Extensions;

public static class PaginationExtension
{
    public static IQueryable<T> Pagination<T>(this IQueryable<T> query, int currentPage, int limit)
    {
        return limit > 0 ? query.Skip((currentPage - 1) * limit).Take(limit) : query;
    }

    public static IEnumerable<T> Pagination<T>(this IEnumerable<T> query, int currentPage, int limit)
    {
        return limit > 0 ? query.Skip((currentPage - 1) * limit).Take(limit) : query;
    }

    public static IFindFluent<T, T> Pagination<T>(this IOrderedFindFluent<T, T> query, int currentPage, int limit)
    {
        return limit > 0 ? query.Skip((currentPage - 1) * limit).Limit(limit) : query.Skip(0);
    }
}