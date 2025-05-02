// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Api.TransferDTOs.Responses;

public class Pagination<T>
{
    public int CurrentPage { get; private set; }
    public int Total { get; set; }
    public int LastPage { get; private set; }
    public IEnumerable<T> Data { get; private set; }
    public int PageSize { get; set; }

    public Pagination(IEnumerable<T> data, int pageSize, int currentPage, int total)
    {
        Data = data;
        PageSize = pageSize;
        CurrentPage = currentPage;
        Total = total;
        LastPage = (int)Math.Ceiling((double)total / pageSize);
    }
}