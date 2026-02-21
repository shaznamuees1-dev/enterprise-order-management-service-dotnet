using OrderManagementService.Domain;

namespace OrderManagementService.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Order order);
    IQueryable<Order> GetQueryable();
}

 