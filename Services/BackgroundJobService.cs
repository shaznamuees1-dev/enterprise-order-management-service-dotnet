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

        // throw new Exception("Simulated job failure");
        // Used temporarily to test Hangfire retry mechanism 

        Log.Information("Background Job: Confirmation sent for Order {OrderId}", orderId);
    }
}