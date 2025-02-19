namespace Api.Extensions;

public static class PaginationExtension
{
    public static IQueryable<T> Pagination<T>(this IQueryable<T> query, int currentPage, int limit)
    {
        return limit > 0 ? query.Skip((currentPage - 1) * limit).Take(limit) : query;
    }
}