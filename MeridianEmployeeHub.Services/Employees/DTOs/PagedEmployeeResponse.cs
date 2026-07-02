namespace MeridianEmployeeHub.Services.Employees.DTOs
{
    // Răspuns paginat pentru GET /api/v1/employees
    // Conține lista de angajați și metadate de paginare.
    public class PagedEmployeeResponse
    {
        public IEnumerable<EmployeeDto> Items { get; set; } = Enumerable.Empty<EmployeeDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}
