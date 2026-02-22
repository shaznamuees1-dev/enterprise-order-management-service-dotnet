namespace OrderManagementService.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}