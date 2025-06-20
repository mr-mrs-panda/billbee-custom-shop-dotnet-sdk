# Billbee Custom Shop SDK

**Connect your custom .NET e-commerce platform to Billbee in minutes, not weeks.**

[![NuGet](https://img.shields.io/nuget/v/Panda.Billbee.CustomShopSdk.svg)](https://www.nuget.org/packages/Panda.Billbee.CustomShopSdk/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A complete .NET SDK that transforms the complex [Billbee Custom Shop API](https://billbee.io) into simple method implementations. No HTTP handling, no authentication complexity, no routing headaches.

> **⚠️ Authorization Required**: The Billbee Custom Shop API requires explicit permission from Billbee GmbH.  
> **⚠️ Unofficial**: This SDK is maintained by Mr. & Mrs. Panda, not Billbee GmbH.

## Why This SDK?

**Without this SDK:**
```csharp
// Complex HTTP request handling
// Manual HMAC-SHA256 authentication
// Parameter parsing and validation
// JSON serialization/deserialization
// Error handling and status codes
// 200+ lines of boilerplate code
```

**With this SDK:**
```csharp
public class MyShop : BillbeeCustomShopService
{
    protected override async Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize)
        => await _database.GetOrdersSince(startDate, page, pageSize);
    
    protected override string GetApiKey() => _config["Billbee:ApiKey"];
}
```

## What Is Billbee?

[Billbee](https://billbee.io) is a multichannel e-commerce management platform that centralizes:
- **Order Management** - Unified inbox for all sales channels
- **Inventory Sync** - Real-time stock across platforms  
- **Automated Fulfillment** - Shipping labels and tracking
- **Financial Management** - Invoicing and accounting integration

While Billbee supports major platforms (Shopify, Amazon, eBay), **custom shops need this API**.

## Installation

```bash
# Core SDK (required)
dotnet add package Panda.Billbee.CustomShopSdk

# ASP.NET Core helpers (recommended)
dotnet add package Panda.Billbee.CustomShopSdk.AspNetCore
```

## Quick Start

### 1. Create Your Service

```csharp
using Panda.Billbee.CustomShopSdk.Services;
using Panda.Billbee.CustomShopSdk.Models;
using Panda.Billbee.CustomShopSdk.Models.Orders;
using Panda.Billbee.CustomShopSdk.Models.Products;

public class MyShopService : BillbeeCustomShopService
{
    private readonly IMyDatabase _database;
    private readonly IConfiguration _config;

    public MyShopService(IMyDatabase database, IConfiguration config)
    {
        _database = database;
        _config = config;
    }

    protected override string GetApiKey() => _config["Billbee:ApiKey"];

    protected override async Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize)
    {
        var orders = await _database.GetOrdersSince(startDate, page, pageSize);
        return new OrderResponse
        {
            Orders = orders.Select(MapToOrder).ToList(),
            Paging = new PagingInfo 
            { 
                Page = page, 
                PageSize = pageSize, 
                TotalCount = await _database.CountOrdersSince(startDate) 
            }
        };
    }

    protected override async Task<Order?> GetOrderAsync(string orderId)
    {
        var order = await _database.GetOrderById(orderId);
        return order == null ? null : MapToOrder(order);
    }

    protected override async Task AckOrderAsync(string orderId)
    {
        await _database.MarkOrderAsAcknowledged(orderId);
    }

    protected override async Task SetOrderStateAsync(SetOrderStateRequest request)
    {
        await _database.UpdateOrderStatus(request.OrderId, request.NewStateId, request.Comment);
        if (!string.IsNullOrEmpty(request.TrackingCode))
            await _database.SetTrackingInfo(request.OrderId, request.TrackingCode, request.TrackingUrl);
    }

    protected override async Task<Product?> GetProductAsync(string productId)
    {
        var product = await _database.GetProductById(productId);
        return product == null ? null : MapToProduct(product);
    }

    protected override async Task<ProductResponse> GetProductsAsync(int page, int pageSize)
    {
        var products = await _database.GetProducts(page, pageSize);
        return new ProductResponse
        {
            Products = products.Select(MapToProduct).ToList(),
            Paging = new PagingInfo { Page = page, PageSize = pageSize, TotalCount = await _database.CountProducts() }
        };
    }

    protected override async Task SetStockAsync(SetStockRequest request)
    {
        if (request.AvailableStock.HasValue)
            await _database.UpdateStock(request.ProductId, request.AvailableStock.Value);
    }

    protected override async Task<List<ShippingProfile>> GetShippingProfilesAsync()
    {
        return new List<ShippingProfile>
        {
            new() { Id = "standard", Name = "Standard Shipping" },
            new() { Id = "express", Name = "Express Shipping" }
        };
    }

    private Order MapToOrder(MyOrderModel order) => new()
    {
        OrderId = order.Id.ToString(),
        OrderNumber = order.Number,
        OrderDate = order.CreatedAt,
        Email = order.CustomerEmail,
        CurrencyCode = order.Currency,
        ShipCost = order.ShippingCost,
        InvoiceAddress = MapToAddress(order.BillingAddress),
        DeliveryAddress = MapToAddress(order.ShippingAddress),
        OrderProducts = order.Items?.Select(MapToOrderProduct).ToList()
    };

    private Product MapToProduct(MyProductModel product) => new()
    {
        Id = product.Id.ToString(),
        Title = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Stock,
        Sku = product.Sku,
        Weight = product.Weight
    };
}
```

### 2. Create Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Panda.Billbee.CustomShopSdk.AspNetCore.Controllers;

[ApiController]
[Route("api/billbee")] // Your webhook URL
public class BillbeeController : BillbeeControllerBase
{
    private readonly MyShopService _shopService;
    
    public BillbeeController(MyShopService shopService) => _shopService = shopService;
    
    protected override IBillbeeCustomShopService BillbeeService => _shopService;
}
```

### 3. Register Services

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<MyShopService>();
builder.Services.AddScoped<IMyDatabase, MyDatabase>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### 4. Configure Billbee

**In your appsettings.json:**
```json
{
  "Billbee": {
    "ApiKey": "your-secret-api-key-from-billbee"
  }
}
```

**In Billbee dashboard:**
1. Settings → Channels → Custom Shop
2. Set URL: `https://yourshop.com/api/billbee`
3. Set the same API key
4. Test connection

**Done!** 🎉 Your shop is now integrated with Billbee.

## API Reference

The SDK automatically handles these endpoints:

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `?Action=GetOrders&StartDate=2024-01-01&Page=1&PageSize=100` | Fetch orders since date |
| GET | `?Action=GetOrder&OrderId=12345` | Get single order |
| GET | `?Action=GetProduct&ProductId=67890` | Get single product |
| GET | `?Action=GetProducts&Page=1&PageSize=50` | Get products list |
| GET | `?Action=GetShippingProfiles` | Get shipping methods |
| POST | `?Action=AckOrder` | Acknowledge order receipt |
| POST | `?Action=SetOrderState` | Update order status |
| POST | `?Action=SetStock` | Update product inventory |

## Core Models

### Order
```csharp
public class Order
{
    public string? OrderId { get; set; }           // Required: Your order ID
    public string? OrderNumber { get; set; }       // Customer-visible number
    public DateTime? OrderDate { get; set; }       // When placed
    public string? Email { get; set; }             // Customer email
    public string? CurrencyCode { get; set; }      // EUR, USD, etc.
    public decimal ShipCost { get; set; }          // Shipping cost
    public Address? InvoiceAddress { get; set; }   // Billing address
    public Address? DeliveryAddress { get; set; }  // Shipping address
    public List<OrderProduct>? OrderProducts { get; set; } // Line items
}
```

### Product
```csharp
public class Product
{
    public string? Id { get; set; }                // Required: Product ID
    public string? Title { get; set; }             // Product name
    public string? Description { get; set; }       // Full description
    public string? Sku { get; set; }               // SKU/Article number
    public decimal? Price { get; set; }            // Current price
    public decimal? Quantity { get; set; }         // Available stock
    public decimal? Weight { get; set; }           // Weight in kg
    public List<ProductImage>? Images { get; set; } // Product images
}
```

## Advanced Usage

### Error Handling & Logging

```csharp
public class BillbeeController : BillbeeControllerBase
{
    public BillbeeController(MyShopService shopService, ILogger<BillbeeController> logger) 
        : base(logger) 
    {
        _shopService = shopService;
    }

    protected override async Task NotifyErrorAsync(ServiceResult serviceResult)
    {
        await base.NotifyErrorAsync(serviceResult); // Default logging
        
        // Custom error handling
        if (serviceResult.ErrorType == ServiceErrorType.InternalServerError)
        {
            await _alertingService.SendAlert(serviceResult.GetErrorMessage());
        }
    }
}
```

### Additional Authentication

```csharp
protected override string GetApiKey() => _config["Billbee:ApiKey"];

protected override (string? Username, string? Password) GetBasicAuthCredentials()
    => (_config["Billbee:Username"], _config["Billbee:Password"]);
```

### Multiple Shops

Create separate service classes and controllers:
- `BookstoreService` with `/bookstore-api` route
- `ElectronicsService` with `/electronics-api` route
- Each with their own API keys and configurations

### Development Mode

```csharp
protected override string? GetApiKey()
{
    #if DEBUG
        return null; // Disable auth in development
    #else
        return _config["Billbee:ApiKey"];
    #endif
}
```

## Troubleshooting

| Error | Solution |
|-------|----------|
| **"Action parameter is required"** | Ensure Billbee sends `?Action=GetOrders` in URL |
| **401 Unauthorized** | Verify API key matches Billbee configuration |
| **404 Not Found** | Check controller route matches Billbee webhook URL |
| **Orders not syncing** | Verify `OrderDate` and date filtering logic |
| **Stock updates failing** | Implement `SetStockAsync` and check ProductId mapping |

## Best Practices

✅ **Always use HTTPS in production**  
✅ **Implement proper async/await patterns**  
✅ **Handle time zones correctly (Billbee uses UTC)**  
✅ **Monitor performance for large datasets**  
✅ **Test with small batches before going live**  
✅ **Log important events (orders exported, stock updated)**  

## Data Flow

```
Billbee ──GET──→ Your API ──→ Your Database
   ↑                ↓               ↓
   │         (Fetch Orders)    (Load Data)
   │                ↓               ↓
   └─────JSON Response ←──── SDK Mapping
```

1. **Billbee** periodically calls your API endpoints
2. **Your API** (via this SDK) routes to your service methods  
3. **Your Service** fetches data from your database
4. **SDK** handles JSON serialization and HTTP responses
5. **Billbee** processes the data for order management

## Migration from v1.x

**Before (complex):**
```csharp
public class MyShop : BillbeeCustomShopService 
{
    protected override IOrderService GetOrderService() => new MyOrderService();
    protected override IProductService GetProductService() => new MyProductService();
}
```

**After (simple):**
```csharp
public class MyShop : BillbeeCustomShopService 
{
    protected override async Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize)
        => await LoadOrdersFromDatabase(startDate, page, pageSize);
    
    // Direct implementation - much cleaner!
}
```

## License & Support

- **License**: MIT
- **Repository**: [billbee-custom-shop-dotnet-sdk](https://github.com/mr-mrs-panda/billbee-custom-shop-dotnet-sdk)
- **Issues**: [GitHub Issues](https://github.com/mr-mrs-panda/billbee-custom-shop-dotnet-sdk/issues)
- **Billbee API Docs**: [billbee.io](https://billbee.io)

> **Legal Notice**: Using the Billbee Custom Shop API requires permission from Billbee GmbH.