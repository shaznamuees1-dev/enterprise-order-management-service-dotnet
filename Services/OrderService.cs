using OrderManagementService.Domain;
using OrderManagementService.Repositories;
using OrderManagementService.DTOs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hangfire;

namespace OrderManagementService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IBackgroundJobClient _backgroundJobs;
    public OrderService(
        IOrderRepository repository,
        IBackgroundJobClient backgroundJobs)
    {
        _repository = repository;
         _backgroundJobs = backgroundJobs ;
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

         
       _backgroundJobs.Enqueue<BackgroundJobService>(x =>
    x.SendOrderConfirmation(created.Id));

        return created;
    }

    public async Task<PagedResult<Order>> GetAllOrdersAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder,
        string? status,
        bool? isVip,
        decimal? minAmount)
    {
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

        var totalRecords = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResult<Order>
        {
            Items = orders,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
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

    public Order? GetNextOrderForProcessing(IEnumerable<Order> orders)
{
    var priorityQueue = new PriorityQueue<Order, (int, decimal, DateTime)>();

    foreach (var order in orders)
    {
        var priority = (
            order.IsVipCustomer ? 0 : 1,        // VIP first
            -order.TotalAmount,                   // Higher amount first
            order.CreatedAt                       // Earlier first
        );
        priorityQueue.Enqueue(order, priority);
    }

    return priorityQueue.Count > 0 
        ? priorityQueue.Dequeue() 
        : null;
}
}