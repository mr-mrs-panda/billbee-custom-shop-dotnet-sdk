# Billbee Custom Shop SDK

A .NET SDK for easy integration with the [Billbee Custom Shop API](https://billbee.io) into custom webshops and e-commerce platforms.
> **Disclaimer:** This repository is **not officially maintained by Billbee GmbH**. It is an independent implementation created and maintained by Mr. & Mrs. Panda, who use and test this SDK in production environments.


## Overview

The Billbee Custom Shop SDK enables seamless connection of custom webshops with the Billbee multichannel software. The SDK handles complete HTTP request processing, parameter parsing, and routing according to the Billbee API specification.

## Features

- ✅ **Complete API Implementation**: All Billbee Custom Shop API endpoints
- ✅ **Automatic Routing**: HTTP request handling and parameter parsing
- ✅ **Secure Authentication**: HMAC-SHA256 API key validation
- ✅ **Flexible Service Integration**: Abstract services for different platforms
- ✅ **Correct HTTP Status Codes**: According to Billbee specification
- ✅ **Minimal Controller Code**: Only a few lines of code required

## Installation

### NuGet Package
```bash
# Via dotnet CLI
dotnet add package Panda.Billbee.CustomShopSdk

# Via Package Manager Console
Install-Package Panda.Billbee.CustomShopSdk

# Via PackageReference in .csproj
<PackageReference Include="Panda.Billbee.CustomShopSdk" Version="1.0.0" />
```

### Local Development
```bash
# For local development
dotnet add reference path/to/Billbee.CustomShopSdk.csproj
```

## Quick Start

### 1. Implement Service Interface

```csharp
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Services;

public interface IMyShopService : IBillbeeCustomShopService
{
    // Add your custom methods here if needed
}

public class MyShopService : BillbeeCustomShopService, IMyShopService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public MyShopService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override IOrderService? GetOrderService()
        => _serviceProvider.GetService<IOrderService>();

    protected override IProductService? GetProductService()
        => _serviceProvider.GetService<IProductService>();

    protected override IStockService? GetStockService()
        => _serviceProvider.GetService<IStockService>();

    protected override IShippingService? GetShippingService()
        => _serviceProvider.GetService<IShippingService>();

    protected override string? GetApiKey()
        => _configuration["Billbee:ApiKey"];
    
    protected override (string? Username, string? Password) GetBasicAuthCredentials()
        => (_configuration["Billbee:BasicAuth:Username"], _configuration["Billbee:BasicAuth:Password"]);
}
```

### 2. Create Controller

```csharp
using Panda.Billbee.CustomShopSdk.Models;
using Panda.Billbee.CustomShopSdk.Constants;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("billbee_api")]
public class BillbeeApiController : ControllerBase
{
    private readonly IMyShopService _service;

    public BillbeeApiController(IMyShopService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> HandleGetRequest([FromQuery] string action, [FromQuery] string? key)
    {
        var request = CreateBillbeeRequest(BillbeeMethods.Get, action, key);
        var result = await _service.HandleRequestAsync(request);
        return ConvertResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> HandlePostRequest([FromQuery] string action, [FromQuery] string? key)
    {
        var request = CreateBillbeeRequest(BillbeeMethods.Post, action, key);
        var result = await _service.HandleRequestAsync(request);
        return ConvertResult(result);
    }

    private BillbeeRequest CreateBillbeeRequest(string method, string? action, string? key)
    {
        var request = new BillbeeRequest
        {
            Method = method,
            Action = action,
            Key = key
        };

        // Add query parameters
        foreach (var param in Request.Query)
            request.QueryParameters[param.Key] = param.Value.FirstOrDefault() ?? string.Empty;

        // Add form parameters (for POST)
        if (method == BillbeeMethods.Post && Request.HasFormContentType)
        {
            foreach (var param in Request.Form)
                request.FormParameters[param.Key] = param.Value.FirstOrDefault() ?? string.Empty;
        }

        return request;
    }

    private IActionResult ConvertResult(ServiceResult result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return result.ErrorType switch
        {
            ServiceErrorType.Unauthorized => Unauthorized(result.ErrorMessage),
            ServiceErrorType.NotFound => NotFound(result.ErrorMessage),
            ServiceErrorType.BadRequest => BadRequest(result.ErrorMessage),
            ServiceErrorType.Forbidden => StatusCode(403, result.ErrorMessage),
            ServiceErrorType.InternalServerError => StatusCode(500, result.ErrorMessage),
            _ => StatusCode(500, result.ErrorMessage)
        };
    }

    private new IActionResult Unauthorized(string? message = null)
    {
        Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Billbee API\"");
        return StatusCode(401, message);
    }
}
```

### 3. Register Services

```csharp
// Program.cs
builder.Services.AddScoped<IOrderService, MyOrderService>();
builder.Services.AddScoped<IProductService, MyProductService>();
builder.Services.AddScoped<IStockService, MyStockService>();
builder.Services.AddScoped<IShippingService, MyShippingService>();
builder.Services.AddScoped<IMyShopService, MyShopService>();
```

## API Endpoints

The SDK implements all Billbee Custom Shop API endpoints:

### GET Requests
- `?Action=GetOrders` - Retrieve orders
- `?Action=GetOrder` - Retrieve single order
- `?Action=GetProduct` - Retrieve single product
- `?Action=GetProducts` - Retrieve product list
- `?Action=GetShippingProfiles` - Retrieve shipping profiles

### POST Requests
- `?Action=AckOrder` - Acknowledge order
- `?Action=SetOrderState` - Change order status
- `?Action=SetStock` - Update inventory

## Service Interfaces

### IOrderService
```csharp
public interface IOrderService
{
    Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize);
    Task<Order?> GetOrderAsync(string orderId);
    Task AckOrderAsync(string orderId);
    Task SetOrderStateAsync(SetOrderStateRequest request);
}
```

### IProductService
```csharp
public interface IProductService
{
    Task<Product?> GetProductAsync(string productId);
    Task<ProductResponse> GetProductsAsync(int page, int pageSize);
}
```

### IStockService
```csharp
public interface IStockService
{
    Task SetStockAsync(SetStockRequest request);
}
```

### IShippingService
```csharp
public interface IShippingService
{
    Task<List<ShippingProfile>> GetShippingProfilesAsync();
}
```

## Data Models

### Order
```csharp
public class Order
{
    public string? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? CurrencyCode { get; set; }
    public string? NickName { get; set; }
    public decimal? ShipCost { get; set; }
    public Address? InvoiceAddress { get; set; }
    public Address? DeliveryAddress { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Email { get; set; }
    public string? Phone1 { get; set; }
    public DateTime? PayDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public OrderStatus? OrderStatusId { get; set; }
    public List<OrderProduct>? OrderProducts { get; set; }
    public List<OrderHistory>? OrderHistory { get; set; }
    public string? SellerComment { get; set; }
    public string? ShippingProfileId { get; set; }
    public string? VatId { get; set; }
}
```

### Product
```csharp
public class Product
{
    public string? Id { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? BasicAttributes { get; set; }
    public string? Title { get; set; }
    public List<ProductImage>? Images { get; set; }
    public decimal? Price { get; set; }
    public decimal? Quantity { get; set; }
    public string? Sku { get; set; }
    public decimal? Weight { get; set; }
    public decimal? VatRate { get; set; }
}
```

## Authentication

The SDK supports Billbee HMAC-SHA256 authentication:

```csharp
// Validate API key
var isValid = ApiKeyAuthenticator.Validate(plainApiKey, receivedKey);
```

Authentication is optional - if no API key is configured, all requests are accepted.

## Advanced Configuration

### Multiple Service Implementations
```csharp
// Different services for different shops
builder.Services.AddKeyedScoped<IOrderService, ShopifyOrderService>("Shopify");
builder.Services.AddKeyedScoped<IOrderService, MagentoOrderService>("Magento");

// In service implementation
protected override IOrderService? GetOrderService()
{
    var shopType = DetermineShopType(); // Custom logic
    return _serviceProvider.GetKeyedService<IOrderService>(shopType);
}
```

### Flexible API Key Resolution
```csharp
protected override string? GetApiKey()
{
    // From HTTP header
    var httpContext = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
    var headerKey = httpContext?.Request.Headers["X-Api-Key"].FirstOrDefault();
    
    // From configuration
    var configKey = _configuration["Billbee:ApiKey"];
    
    // From database based on request
    var dbKey = GetApiKeyFromDatabase(httpContext?.Request);
    
    return headerKey ?? configKey ?? dbKey;
}
```

## Billbee API Specification

### HTTP Status Codes
- **200 OK**: Request processed successfully
- **400 Bad Request**: Invalid request format or missing parameters
- **401 Unauthorized**: Authentication failed
- **403 Forbidden**: Successfully authenticated but no authorization
- **404 Not Found**: Resource does not exist
- **500 Internal Server Error**: Unexpected server error

### Request Format
- **GET**: Parameters in URL (`?Action=GetOrders&StartDate=2023-01-01`)
- **POST**: Action in URL, parameters in form body

### API Key Format
```php
// Billbee API key generation (PHP reference)
$timestamp = substr(time(), 0, 7);
$hash = hash_hmac("sha256", utf8_encode($apiPassword), utf8_encode($timestamp));
$key = str_replace(["=", "/", "+"], "", base64_encode($hash));
```

## Troubleshooting

### Common Issues

**Controller not found**
```csharp
// Ensure controllers are registered
builder.Services.AddControllers();
app.MapControllers();
```

**API key validation fails**
```csharp
// Check timestamp synchronization
// Billbee uses UTC time with 7-digit Unix timestamp
```

**Services not found**
```csharp
// Register all required services in DI
builder.Services.AddScoped<IMyShopService, MyShopService>();
```

## Legal Notice

> **Note:** The use of this interface requires permission from Billbee GmbH.

## Support

- **GitHub Repository**: [billbee-custom-shop-dotnet-sdk](https://github.com/mr-mrs-panda/billbee-custom-shop-dotnet-sdk)
- **Issues**: [GitHub Issues](https://github.com/mr-mrs-panda/billbee-custom-shop-dotnet-sdk/issues)
- **Billbee API Documentation**: [Custom Shop API](https://billbee.io)
- **Billbee Support**: Contact Billbee directly for API questions