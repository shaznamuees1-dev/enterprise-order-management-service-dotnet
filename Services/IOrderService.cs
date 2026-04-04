using OrderManagementService.Domain;
using OrderManagementService.DTOs;

namespace OrderManagementService.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);

    Task<PagedResult<Order>> GetAllOrdersAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder,
        string? status,
        bool? isVip,
        decimal? minAmount);

    Order? GetNextOrderForProcessing(IEnumerable<Order> orders);
    
    Task<Order?> GetOrderByIdAsync(int id);

    Task<Order?> UpdateOrderAsync(int id, Order updatedOrder);

    Task<bool> DeleteOrderAsync(int id);
}