namespace MiniTicker.Core.Application.Shared
{
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}