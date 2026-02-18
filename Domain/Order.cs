namespace OrderManagementService.Domain;

public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public bool IsVipCustomer { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
