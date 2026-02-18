using OrderManagementService.Domain;
using OrderManagementService.Repositories;

namespace OrderManagementService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        return await _repository.CreateAsync(order);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await _repository.UpdateAsync(order);
    }
}
