using OrderManagementService.Domain;
using OrderManagementService.Repositories;
using Microsoft.EntityFrameworkCore;
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
    order.CreatedAt = DateTime.UtcNow;

     
    if (order.TotalAmount > 10000)
    {
        order.Status = OrderStatus.Approved;
    }

  
    if (order.IsVipCustomer)
    {
        order.Status = OrderStatus.Priority;
    }

    return await _repository.CreateAsync(order);
}


   public async Task<List<Order>> GetAllOrdersAsync(
    int page,
    int pageSize,
    string? sortBy,
    string sortOrder)
{
    var query = _repository.GetQueryable();

    // Sorting
    if (!string.IsNullOrEmpty(sortBy))
    {
        query = sortBy.ToLower() switch
        {
            "customername" => sortOrder == "desc"
                ? query.OrderByDescending(o => o.CustomerName)
                : query.OrderBy(o => o.CustomerName),

            "totalamount" => sortOrder == "desc"
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),

            _ => query
        };
    }

    var orders = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return orders;
}    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

 public async Task<Order?> UpdateOrderAsync(int id, Order updatedOrder)
{
    var existingOrder = await _repository.GetByIdAsync(id);

    if (existingOrder == null)
        return null;

    if (existingOrder.Status == OrderStatus.Completed)
        throw new InvalidOperationException("Completed orders cannot be modified.");

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
