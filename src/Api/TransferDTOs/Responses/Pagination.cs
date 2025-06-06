// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Api.TransferDTOs.Responses;

public class Pagination<T>(IEnumerable<T> data, int pageSize, int currentPage, long total)
{
    public int CurrentPage { get; private set; } = currentPage;
    public long Total { get; set; } = total;
    public int LastPage { get; private set; } = (int)Math.Ceiling((double)total / pageSize);
    public IEnumerable<T> Data { get; private set; } = data;
    public int PageSize { get; set; } = pageSize;
}