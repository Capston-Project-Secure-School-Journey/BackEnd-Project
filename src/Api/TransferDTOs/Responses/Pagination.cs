// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Api.TransferDTOs.Responses;

public class Pagination<T>
{
    public int CurrentPage { get; private set; }
    private int _pageSize;
    public int Total { get; set; }
    public int LastPage { get; private set; }
    public IEnumerable<T> Data { get; private set; }

    private int PageSize
    {
        set => _pageSize = value;
    }

    public Pagination(IEnumerable<T> data, int pageSize, int currentPage, int total)
    {
        Data = data;
        PageSize = pageSize;
        CurrentPage = currentPage;
        Total = total;
        LastPage = (int)Math.Ceiling((double)total / _pageSize);
    }
}