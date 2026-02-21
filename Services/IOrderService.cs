using OrderManagementService.Domain;

namespace OrderManagementService.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);
    Task<List<Order>> GetAllOrdersAsync(
    int page,
    int pageSize,
    string? sortBy,
    string sortOrder);
    Task<Order?> GetOrderByIdAsync(int id);
    
    Task<Order?> UpdateOrderAsync(int id, Order updatedOrder);
    Task<bool> DeleteOrderAsync(int id);

}
