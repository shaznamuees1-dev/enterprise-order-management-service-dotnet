using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderManagementService.Domain;
using OrderManagementService.Services;
using OrderManagementService.DTOs;
using System.Security.Claims;
using Hangfire;

namespace OrderManagementService.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly ILogger<OrderController> _logger;
    private readonly IBackgroundJobClient _backgroundJobs; 

  

    private readonly IRecurringJobManager _recurringJobs;
    public OrderController(
     IOrderService service,
     ILogger<OrderController> logger,
     IBackgroundJobClient backgroundJobs,
     IRecurringJobManager recurringJobs)
    {
        _service = service;
        _logger = logger;
        _backgroundJobs = backgroundJobs;
        _recurringJobs = recurringJobs;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<ActionResult<BaseResponse<OrderResponse>>> CreateOrder(CreateOrderRequest request)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} is creating order for {CustomerName}", userId, request.CustomerName);

        var order = new Order
        {
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount,
            IsVipCustomer = request.IsVipCustomer
        };

        var createdOrder = await _service.CreateOrderAsync(order);

        _logger.LogInformation("User {UserId} created order {OrderId}", userId, createdOrder.Id);

        var response = new OrderResponse
        {
            Id = createdOrder.Id,
            CustomerName = createdOrder.CustomerName,
            TotalAmount = createdOrder.TotalAmount,
            IsVipCustomer = createdOrder.IsVipCustomer,
            Status = createdOrder.Status,
            CreatedAt = createdOrder.CreatedAt
        };
        
        _backgroundJobs.Enqueue<BackgroundJobService>(
            x => x.SendOrderCreatedNotification(createdOrder.Id));

        _backgroundJobs.Schedule<BackgroundJobService>(
            x => x.ProcessOrder(createdOrder.Id),
            TimeSpan.FromSeconds(30));

        return CreatedAtAction(nameof(GetOrderById), new { id = response.Id },
            new BaseResponse<OrderResponse>
            {
                Success = true,
                Message = "Order created successfully.",
                Data = response
            });
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<OrderResponse>>>> GetAllOrders(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortOrder = "asc",
        string? status = null,
        bool? isVip = null,
        decimal? minAmount = null)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} is fetching orders (Page: {Page}, PageSize: {PageSize})", userId, page, pageSize);

        var pagedResult = await _service.GetAllOrdersAsync(
            page, pageSize, sortBy, sortOrder, status, isVip, minAmount);

        var mappedItems = pagedResult.Items.Select(o => new OrderResponse
        {
            Id = o.Id,
            CustomerName = o.CustomerName,
            TotalAmount = o.TotalAmount,
            IsVipCustomer = o.IsVipCustomer,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        }).ToList();

        return Ok(new BaseResponse<PagedResult<OrderResponse>>
        {
            Success = true,
            Message = "Orders retrieved successfully.",
            Data = new PagedResult<OrderResponse>
            {
                Items = mappedItems,
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages
            }
        });
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<OrderResponse>>> GetOrderById(int id)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} is fetching order {OrderId}", userId, id);

        var order = await _service.GetOrderByIdAsync(id);

        if (order == null)
        {
            _logger.LogWarning("User {UserId} tried to access non-existing order {OrderId}", userId, id);
            return NotFound();
        }

        var response = new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            IsVipCustomer = order.IsVipCustomer,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        return Ok(new BaseResponse<OrderResponse>
        {
            Success = true,
            Message = "Order retrieved successfully.",
            Data = response
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponse<OrderResponse>>> UpdateOrder(int id,[FromBody] UpdateOrderRequest request)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} is updating order {OrderId}", userId, id);

        var updatedOrder = new Order
        {
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount,
            IsVipCustomer = request.IsVipCustomer,
            Status = request.Status
        };

        var result = await _service.UpdateOrderAsync(id, updatedOrder);

        if (result == null)
        {
            _logger.LogWarning("User {UserId} failed to update order {OrderId} (not found)", userId, id);
            return NotFound();
        }

        _logger.LogInformation("User {UserId} updated order {OrderId}", userId, id);

        var response = new OrderResponse
        {
            Id = result.Id,
            CustomerName = result.CustomerName,
            TotalAmount = result.TotalAmount,
            IsVipCustomer = result.IsVipCustomer,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };

        return Ok(new BaseResponse<OrderResponse>
        {
            Success = true,
            Message = "Order updated successfully.",
            Data = response
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponse<object>>> DeleteOrder(int id)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} is deleting order {OrderId}", userId, id);

        var success = await _service.DeleteOrderAsync(id);

        if (!success)
        {
            _logger.LogWarning("User {UserId} failed to delete order {OrderId} (not found)", userId, id);
            return NotFound();
        }

        _logger.LogInformation("User {UserId} deleted order {OrderId}", userId, id);

        return Ok(new BaseResponse<object>
        {
            Success = true,
            Message = "Order deleted successfully.",
            Data = null
        });
    }
    [HttpGet("next-for-processing")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNextOrderForProcessing()
    {
        var orders = await _service.GetAllOrdersAsync(1, 100, null, "asc", null, null, null);
        var next = _service.GetNextOrderForProcessing(orders.Items);
    
        if (next == null)
        return NotFound(new { message = "No orders available for processing" });
        
        return Ok(new { message = "Next order to process", data = next });
    }
}