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

   public async Task<Order?> UpdateOrderAsync(int id, Order updatedOrder)
{
    var existingOrder = await _repository.GetByIdAsync(id);

    if (existingOrder == null)
        return null;

    existingOrder.CustomerName = updatedOrder.CustomerName;
    existingOrder.TotalAmount = updatedOrder.TotalAmount;
    existingOrder.IsVipCustomer = updatedOrder.IsVipCustomer;
    existingOrder.Status = updatedOrder.Status;

    await _repository.UpdateAsync(existingOrder);

    return existingOrder;
}

public async Task<bool> DeleteOrderAsync(int id)
{
    var order = await _repository.GetByIdAsync(id);

    if (order == null)
        return false;

    await _repository.DeleteAsync(order);
    return true;
}

}
