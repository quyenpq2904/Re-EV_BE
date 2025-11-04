namespace ReEV.Common
{
    public class PaginationResult<T> where T : class
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int Pagesize { get; set; }
        public List<T> Items { get; set; }

        public PaginationResult(int totalCount, int totalPages, int page, int pageSize, List<T> items)
        {
            TotalCount = totalCount;
            TotalPages = totalPages;
            Page = page;
            Pagesize = pageSize;
            Items = items;
        }

        public PaginationResult()
        {
        }
    }
}
