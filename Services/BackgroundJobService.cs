using Serilog;
using Hangfire;

namespace OrderManagementService.Services;

[AutomaticRetry(Attempts = 3)]
public class BackgroundJobService
{
    public async Task SendOrderConfirmation(int orderId)
    {
        Log.Information("Background Job: Sending confirmation for Order {OrderId}", orderId);

        await Task.Delay(2000);

         

        Log.Information("Background Job: Confirmation sent for Order {OrderId}", orderId);
    }

public void SendOrderCreatedNotification(int orderId)
{
    Log.Information("Background Job: Notification sent for Order {OrderId} at {Time}", 
        orderId, DateTime.Now);
}

public void ProcessOrder(int orderId)
{
    Log.Information("Background Job: Processing Order {OrderId} at {Time}", 
        orderId, DateTime.Now);
}
}