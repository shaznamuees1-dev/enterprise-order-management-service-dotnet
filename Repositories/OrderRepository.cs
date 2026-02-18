using Microsoft.EntityFrameworkCore;
using OrderManagementService.Data;
using OrderManagementService.Domain;

namespace OrderManagementService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
{
    return await _context.Orders.FindAsync(id);
}

public async Task UpdateAsync(Order order)
{
    _context.Orders.Update(order);
    await _context.SaveChangesAsync();
}

public async Task DeleteAsync(Order order)
{
    _context.Orders.Remove(order);
    await _context.SaveChangesAsync();
}
}