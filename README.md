# Billbee Custom Shop SDK

A .NET SDK for easy integration with the [Billbee Custom Shop API](https://billbee.io) into custom webshops and e-commerce platforms.

> **Important:** The use of the Billbee Custom Shop API requires explicit permission from Billbee GmbH. This interface enables direct integration between your webshop and Billbee's multichannel e-commerce management platform.

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

## What is the Billbee Custom Shop API?

The **Billbee Custom Shop API** is a programming interface that enables seamless data exchange between custom webshops/marketplaces and [Billbee](https://www.billbee.io), a powerful multichannel e-commerce management software. 

### Why Use This API?

**Billbee** is a comprehensive solution that helps online merchants manage their business across multiple sales channels. It provides:
- **Multichannel Order Management**: Centralize orders from multiple platforms
- **Inventory Synchronization**: Keep stock levels synchronized across all channels
- **Automated Fulfillment**: Streamline shipping and order processing
- **Financial Management**: Handle invoicing, taxes, and accounting
- **Analytics & Reporting**: Gain insights into your business performance

### The Integration Challenge

While Billbee supports many popular e-commerce platforms out-of-the-box (Shopify, WooCommerce, Amazon, eBay, etc.), **custom webshops** and **proprietary e-commerce solutions** need a direct integration pathway. This is where the Custom Shop API comes in.

### How This SDK Helps

**Without this SDK**, implementing the Billbee Custom Shop API requires:
- Deep understanding of the API specification
- Manual HTTP request/response handling
- Complex HMAC-SHA256 authentication implementation
- Proper error handling and status code management
- Request routing and parameter parsing
- JSON serialization/deserialization

**With this SDK**, you get:
- ✅ **One-line integration**: Just implement your business logic
- ✅ **Automatic request handling**: All HTTP complexity abstracted away
- ✅ **Secure authentication**: HMAC-SHA256 validation built-in
- ✅ **Type-safe models**: Strongly typed C# classes for all data structures
- ✅ **Standard ASP.NET Core**: Fits naturally into existing applications
- ✅ **Production-ready**: Used and tested in real e-commerce environments

### Data Flow Overview

```
Your Webshop  ←→  This SDK  ←→  Billbee Platform
     ↓               ↓              ↓
- Orders         - HTTP API     - Order Management
- Products       - Security     - Inventory Sync  
- Inventory      - Routing      - Fulfillment
- Customers      - Models       - Reporting
```

**Billbee acts as the client**, making HTTPS requests to your webshop to:
- **Fetch new orders** and customer data for processing
- **Retrieve product information** for catalog management  
- **Update inventory levels** when stock changes on other channels
- **Synchronize order status** when orders are shipped or fulfilled

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

### 1. Implement Service Interfaces

```csharp
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Services;

// Bookstore Service
public interface IBookstoreService : IBillbeeCustomShopService
{
    // Add your custom methods here if needed
}

public class BookstoreService : BillbeeCustomShopService, IBookstoreService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public BookstoreService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override IOrderService? GetOrderService()
        => _serviceProvider.GetKeyedService<IOrderService>("Bookstore");

    protected override IProductService? GetProductService()
        => _serviceProvider.GetKeyedService<IProductService>("Bookstore");

    protected override IStockService? GetStockService()
        => _serviceProvider.GetKeyedService<IStockService>("Bookstore");

    protected override IShippingService? GetShippingService()
        => _serviceProvider.GetKeyedService<IShippingService>("Bookstore");

    protected override string? GetApiKey()
        => _configuration["Billbee:Bookstore:ApiKey"];
    
    protected override (string? Username, string? Password) GetBasicAuthCredentials()
        => (_configuration["Billbee:Bookstore:Username"], _configuration["Billbee:Bookstore:Password"]);
}

// Electronics Store Service
public interface IElectronicsStoreService : IBillbeeCustomShopService { }

public class ElectronicsStoreService : BillbeeCustomShopService, IElectronicsStoreService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ElectronicsStoreService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override IOrderService? GetOrderService()
        => _serviceProvider.GetKeyedService<IOrderService>("Electronics");

    protected override IProductService? GetProductService()
        => _serviceProvider.GetKeyedService<IProductService>("Electronics");

    protected override IStockService? GetStockService()
        => _serviceProvider.GetKeyedService<IStockService>("Electronics");

    protected override IShippingService? GetShippingService()
        => _serviceProvider.GetKeyedService<IShippingService>("Electronics");

    protected override string? GetApiKey()
        => _configuration["Billbee:Electronics:ApiKey"];
    
    protected override (string? Username, string? Password) GetBasicAuthCredentials()
        => (_configuration["Billbee:Electronics:Username"], _configuration["Billbee:Electronics:Password"]);
}
```

### 2. Create Controllers

```csharp
using Panda.Billbee.CustomShopSdk.Models;
using Panda.Billbee.CustomShopSdk.Constants;
using Microsoft.AspNetCore.Mvc;

// Bookstore Controller
[ApiController]
[Route("bookstore_api")]
public class BookstoreController : ControllerBase
{
    private readonly IBookstoreService _service;

    public BookstoreController(IBookstoreService service)
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

// Electronics Store Controller
[ApiController]
[Route("electronics_api")]
public class ElectronicsStoreController : ControllerBase
{
    private readonly IElectronicsStoreService _service;

    public ElectronicsStoreController(IElectronicsStoreService service)
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

    // Same helper methods as BookstoreController (CreateBillbeeRequest, ConvertResult, Unauthorized)
    // Implementation details omitted for brevity
}
```

### 3. Register Services

```csharp
// Program.cs

// Bookstore services
builder.Services.AddKeyedScoped<IOrderService, BookstoreOrderService>("Bookstore");
builder.Services.AddKeyedScoped<IProductService, BookstoreProductService>("Bookstore");
builder.Services.AddKeyedScoped<IStockService, BookstoreStockService>("Bookstore");
builder.Services.AddKeyedScoped<IShippingService, BookstoreShippingService>("Bookstore");

// Electronics store services
builder.Services.AddKeyedScoped<IOrderService, ElectronicsOrderService>("Electronics");
builder.Services.AddKeyedScoped<IProductService, ElectronicsProductService>("Electronics");
builder.Services.AddKeyedScoped<IStockService, ElectronicsStockService>("Electronics");
builder.Services.AddKeyedScoped<IShippingService, ElectronicsShippingService>("Electronics");

// Billbee services
builder.Services.AddScoped<IBookstoreService, BookstoreService>();
builder.Services.AddScoped<IElectronicsStoreService, ElectronicsStoreService>();
```

### 4. Configuration

```json
{
  "Billbee": {
    "Bookstore": {
      "ApiKey": "your-bookstore-api-key",
      "Username": "bookstore-user",
      "Password": "bookstore-password"
    },
    "Electronics": {
      "ApiKey": "your-electronics-api-key",
      "Username": "electronics-user",
      "Password": "electronics-password"
    }
  }
}
```

## API Endpoints

The SDK implements all Billbee Custom Shop API endpoints. Each shop has its own dedicated endpoint:

### Example URLs

**Bookstore API (Route: `/bookstore_api`)**
- `GET /bookstore_api?Action=GetOrders&StartDate=2024-01-01&Page=1&PageSize=100`
- `GET /bookstore_api?Action=GetOrder&OrderId=12345`
- `POST /bookstore_api?Action=AckOrder` (OrderId in form body)
- `POST /bookstore_api?Action=SetOrderState` (Status data in form body)

**Electronics Store API (Route: `/electronics_api`)**
- `GET /electronics_api?Action=GetProducts&Page=1&PageSize=50`
- `GET /electronics_api?Action=GetProduct&ProductId=67890`
- `POST /electronics_api?Action=SetStock` (Stock data in form body)

### Supported Actions

#### GET Requests
- `?Action=GetOrders` - Retrieve orders with pagination and date filtering
- `?Action=GetOrder` - Retrieve single order by ID
- `?Action=GetProduct` - Retrieve single product by ID
- `?Action=GetProducts` - Retrieve product list with pagination
- `?Action=GetShippingProfiles` - Retrieve available shipping profiles

#### POST Requests
- `?Action=AckOrder` - Acknowledge successful order import
- `?Action=SetOrderState` - Update order status (shipped, paid, etc.)
- `?Action=SetStock` - Update product inventory levels

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