using Microsoft.AspNetCore.Mvc;
using OrderManagementService.Domain;
using OrderManagementService.Services;
using OrderManagementService.DTOs;
using System.Linq;

namespace OrderManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _service;

    public OrderController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateOrder(CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount,
            IsVipCustomer = request.IsVipCustomer
        };

        var createdOrder = await _service.CreateOrderAsync(order);

        var response = new OrderResponse
        {
            Id = createdOrder.Id,
            CustomerName = createdOrder.CustomerName,
            TotalAmount = createdOrder.TotalAmount,
            IsVipCustomer = createdOrder.IsVipCustomer,
            Status = createdOrder.Status,
            CreatedAt = createdOrder.CreatedAt
        };

        var apiResponse = new ApiResponse<OrderResponse>
        {
            Success = true,
            Message = "Order created successfully.",
            Data = response
        };

        return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, apiResponse);
    }

    [HttpGet]
    [ResponseCache(Duration = 30)]
    public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetAllOrders(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortOrder = "asc",
        string? status = null,
        bool? isVip = null,
        decimal? minAmount = null)
    {
        var orders = await _service.GetAllOrdersAsync(page, pageSize, sortBy, sortOrder, status, isVip, minAmount);

        var response = orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            CustomerName = o.CustomerName,
            TotalAmount = o.TotalAmount,
            IsVipCustomer = o.IsVipCustomer,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        }).ToList();

        var apiResponse = new ApiResponse<List<OrderResponse>>
        {
            Success = true,
            Message = "Orders retrieved successfully.",
            Data = response
        };

        return Ok(apiResponse);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetOrderById(int id)
    {
        var order = await _service.GetOrderByIdAsync(id);

        if (order == null)
            return NotFound();

        var response = new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            IsVipCustomer = order.IsVipCustomer,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        var apiResponse = new ApiResponse<OrderResponse>
        {
            Success = true,
            Message = "Order retrieved successfully.",
            Data = response
        };

        return Ok(apiResponse);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var updatedOrder = new Order
        {
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount,
            IsVipCustomer = request.IsVipCustomer,
            Status = request.Status
        };

        var result = await _service.UpdateOrderAsync(id, updatedOrder);

        if (result == null)
            return NotFound();

        var response = new OrderResponse
        {
            Id = result.Id,
            CustomerName = result.CustomerName,
            TotalAmount = result.TotalAmount,
            IsVipCustomer = result.IsVipCustomer,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };

        var apiResponse = new ApiResponse<OrderResponse>
        {
            Success = true,
            Message = "Order updated successfully.",
            Data = response
        };

        return Ok(apiResponse);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOrder(int id)
    {
        var success = await _service.DeleteOrderAsync(id);

        if (!success)
            return NotFound();

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Message = "Order deleted successfully.",
            Data = null
        };

        return Ok(apiResponse);
    }
}