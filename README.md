# Canonical Logger for .NET

Inspired by Observability 2.0, Canonical Logger collects logs throughout a request lifecycle and dumps them into one log event.

## Setup    
Add to your Program.cs:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add your usual logging providers
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry();

// Add canonical logging
builder.Services.AddCanonicalLogging();

var app = builder.Build();
app.UseCanonicalLogging();
```

## Usage
Use standard ILogger interfaces - logs are automatically collected:

```csharp
public class OrderController : ControllerBase 
{
    private readonly ILogger<OrderController> _logger;
    
    public async Task<IActionResult> ProcessOrder(int orderId)
    {
        using (_logger.BeginScope("Processing Order {OrderId}", orderId))
        {
            _logger.LogInformation("Starting order processing");
            await _orderService.Process(orderId);
            _logger.LogInformation("Order processed successfully");
        }
    }
}
```

## Log Outputs
JSON Format

```json
[
  {
    "LogLevel": "Information",
    "Message": "Starting order processing",
    "State": {
      "OrderId": 12345,
      "{OriginalFormat}": "Starting order processing"
    },
    "Scope": "Processing Order 12345"
  },
  {
    "LogLevel": "Information",
    "Message": "Order processed successfully",
    "State": {
      "OrderId": 12345,
      "{OriginalFormat}": "Order processed successfully"
    },
    "Scope": "Processing Order 12345"
  }
]
```

Console
```
--- Canonical Log Summary for OrderController ---
[2024-12-06 14:23:45.123] [Information] [Processing Order 12345] Starting order processing
[2024-12-06 14:23:46.456] [Information] [Processing Order 12345] Order processed successfully
--- End of Canonical Log ---
```

## Source Generated Logging
Use the LoggerMessage attribute for better performance:

```csharp
public static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Processing order {OrderId} for customer {CustomerName}")]
    public static partial void LogOrderProcessing(
        this ILogger logger,
        int orderId, 
        string customerName);
}

// Usage:
_logger.LogOrderProcessing(12345, "John Smith");
```
