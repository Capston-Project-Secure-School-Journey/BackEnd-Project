using System.Linq.Dynamic.Core;

namespace Api.Extensions;

public static class EnumerableExtension
{
    public static IOrderedQueryable<T> SortByProperty<T>(this IQueryable<T> source, string propertyName,
        bool ascending = true)
    {
        var propInfo = typeof(T).GetProperty(propertyName);
        if (propInfo == null)
            throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(T).Name}'");

        return source.OrderBy(propertyName, ascending ? "ASC" : "DESC");
    }

    public static IOrderedQueryable<T> SortByProperty<T>(this IQueryable<T> source, string propertyName,
        string direction)
    {
        var propInfo = typeof(T).GetProperty(propertyName);
        if (propInfo == null)
            throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(T).Name}'");

        if (direction != "DESC" && direction != "ASC")
            throw new ArgumentException($"Direction '{direction}' is not supported");

        return source.OrderBy(propertyName, direction);
    }
}