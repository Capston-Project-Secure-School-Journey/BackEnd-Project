namespace Api.DTOs.Responses
{
    public class Pagination<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int LastPage { get; set; }
        public IEnumerable<T> Data { get; set; }
        
        public Pagination(IEnumerable<T> data, int pageSize, int currentPage, int total)
        {
            Data = data;
            PageSize = pageSize;
            CurrentPage = currentPage;
            Total = total;
            LastPage = (int)Math.Ceiling((double)total / PageSize);
        }
    }
}
