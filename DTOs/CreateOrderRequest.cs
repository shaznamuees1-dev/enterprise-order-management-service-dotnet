namespace OrderManagementService.DTOs;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsVipCustomer { get; set; }
}
