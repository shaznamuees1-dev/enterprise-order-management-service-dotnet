namespace OrderManagementService.DTOs;
using System.ComponentModel.DataAnnotations;
using OrderManagementService.Domain;

public class CreateOrderRequest
{
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Range(1, 100000)]
    public decimal TotalAmount { get; set; }

    public bool IsVipCustomer { get; set; }
}