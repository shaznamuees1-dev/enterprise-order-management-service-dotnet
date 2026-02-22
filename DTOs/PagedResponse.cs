namespace OrderManagementService.DTOs;

public class PagedResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T Data { get; set; } = default!;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}