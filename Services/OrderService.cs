using OrderManagementService.Domain;
using OrderManagementService.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
        Log.Information("Creating order for customer {CustomerName} with amount {Amount}",
            order.CustomerName, order.TotalAmount);

        order.CreatedAt = DateTime.UtcNow;

        if (order.TotalAmount > 10000)
        {
            order.Status = OrderStatus.Approved;
            Log.Information("Order automatically approved due to high amount.");
        }

        if (order.IsVipCustomer)
        {
            order.Status = OrderStatus.Priority;
            Log.Information("Order marked as Priority for VIP customer.");
        }

        var created = await _repository.CreateAsync(order);

        Log.Information("Order {OrderId} created successfully.", created.Id);

        return created;
    }

    public async Task<List<Order>> GetAllOrdersAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder,
        string? status,
        bool? isVip,
        decimal? minAmount)
    {
        Log.Information("Fetching orders with filters: Page={Page}, PageSize={PageSize}, Status={Status}, IsVip={IsVip}, MinAmount={MinAmount}",
            page, pageSize, status, isVip, minAmount);

       var query = _repository
           .GetQueryable()
           .AsNoTracking();
       
        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(o => o.Status == parsedStatus);
        }

        if (isVip.HasValue)
        {
            query = query.Where(o => o.IsVipCustomer == isVip.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount >= minAmount.Value);
        }

         
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

        Log.Information("Returned {Count} orders.", orders.Count);

        return orders;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        Log.Information("Fetching order with ID {OrderId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Order?> UpdateOrderAsync(int id, Order updatedOrder)
    {
        Log.Information("Updating order {OrderId}", id);

        var existingOrder = await _repository.GetByIdAsync(id);

        if (existingOrder == null)
        {
            Log.Warning("Order {OrderId} not found.", id);
            return null;
        }

        if (existingOrder.Status == OrderStatus.Completed)
        {
            Log.Warning("Attempt to update completed order {OrderId}", id);
            throw new InvalidOperationException("Completed orders cannot be modified.");
        }

        existingOrder.CustomerName = updatedOrder.CustomerName;
        existingOrder.TotalAmount = updatedOrder.TotalAmount;
        existingOrder.IsVipCustomer = updatedOrder.IsVipCustomer;
        existingOrder.Status = updatedOrder.Status;

        await _repository.UpdateAsync(existingOrder);

        Log.Information("Order {OrderId} updated successfully.", id);

        return existingOrder;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        Log.Information("Deleting order {OrderId}", id);

        var order = await _repository.GetByIdAsync(id);

        if (order == null)
        {
            Log.Warning("Order {OrderId} not found for deletion.", id);
            return false;
        }

        await _repository.DeleteAsync(order);

        Log.Information("Order {OrderId} deleted successfully.", id);

        return true;
    }
}