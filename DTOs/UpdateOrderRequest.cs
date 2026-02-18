namespace OrderManagementService.DTOs;
using OrderManagementService.Domain;

public class UpdateOrderRequest
{
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsVipCustomer { get; set; }
    public OrderStatus Status { get; set; }

}
