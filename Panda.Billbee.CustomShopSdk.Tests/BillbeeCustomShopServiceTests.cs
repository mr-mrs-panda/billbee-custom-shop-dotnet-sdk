using Moq;
using Panda.Billbee.CustomShopSdk.Constants;
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Models;
using Panda.Billbee.CustomShopSdk.Models.Orders;
using Panda.Billbee.CustomShopSdk.Models.Products;
using Panda.Billbee.CustomShopSdk.Models.Shipping;
using Panda.Billbee.CustomShopSdk.Services;

namespace Panda.Billbee.CustomShopSdk.Tests;

public class BillbeeCustomShopServiceTests
{
    private class TestBillbeeCustomShopService : BillbeeCustomShopService
    {
        public IOrderService? OrderService { get; set; }
        public IProductService? ProductService { get; set; }
        public IStockService? StockService { get; set; }
        public IShippingService? ShippingService { get; set; }
        public string? ApiKey { get; set; }
        public (string? Username, string? Password) BasicAuthCredentials { get; set; } = (null, null);

        protected override IOrderService? GetOrderService() => OrderService;
        protected override IProductService? GetProductService() => ProductService;
        protected override IStockService? GetStockService() => StockService;
        protected override IShippingService? GetShippingService() => ShippingService;
        protected override string? GetApiKey() => ApiKey;
        protected override (string? Username, string? Password) GetBasicAuthCredentials() => BasicAuthCredentials;
    }

    private readonly TestBillbeeCustomShopService _service;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IStockService> _mockStockService;
    private readonly Mock<IShippingService> _mockShippingService;

    public BillbeeCustomShopServiceTests()
    {
        _service = new TestBillbeeCustomShopService();
        _mockOrderService = new Mock<IOrderService>();
        _mockProductService = new Mock<IProductService>();
        _mockStockService = new Mock<IStockService>();
        _mockShippingService = new Mock<IShippingService>();

        _service.OrderService = _mockOrderService.Object;
        _service.ProductService = _mockProductService.Object;
        _service.StockService = _mockStockService.Object;
        _service.ShippingService = _mockShippingService.Object;
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = null
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.BadRequest, result.ErrorType);
        Assert.Equal("Action parameter is required", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithEmptyAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = string.Empty
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.BadRequest, result.ErrorType);
        Assert.Equal("Action parameter is required", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithApiKeyValidationFailure_ReturnsUnauthorized()
    {
        // Arrange
        _service.ApiKey = "valid-api-key";
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders,
            Key = "invalid-key"
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public async Task HandleRequestAsync_WithBasicAuthValidationFailure_ReturnsUnauthorized()
    {
        // Arrange
        _service.BasicAuthCredentials = ("user", "pass");
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders,
            AuthorizationHeader = "Basic invalid"
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public async Task HandleRequestAsync_WithInvalidAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = "invalid-action"
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.BadRequest, result.ErrorType);
        Assert.Contains("Invalid action", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_GetOrders_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var expectedOrders = new OrderResponse
        {
            Orders = new List<Order> { new Order { OrderId = "123" } },
            Paging = new PagingInfo { Page = 1, TotalCount = 1 }
        };
        _mockOrderService.Setup(x => x.GetOrdersAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedOrders);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders,
            QueryParameters = { { BillbeeQueryParameters.StartDate, "2023-01-01" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedOrders, result.Data);
    }

    [Fact]
    public async Task HandleRequestAsync_GetOrders_WithDefaultParameters_UsesDefaults()
    {
        // Arrange
        var expectedOrders = new OrderResponse();
        _mockOrderService.Setup(x => x.GetOrdersAsync(It.IsAny<DateTime>(), 1, 100))
            .ReturnsAsync(expectedOrders);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _mockOrderService.Verify(x => x.GetOrdersAsync(It.IsAny<DateTime>(), 1, 100), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_GetOrder_WithValidOrderId_ReturnsSuccess()
    {
        // Arrange
        var expectedOrder = new Order { OrderId = "123" };
        _mockOrderService.Setup(x => x.GetOrderAsync("123"))
            .ReturnsAsync(expectedOrder);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrder,
            QueryParameters = { { BillbeeQueryParameters.OrderId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedOrder, result.Data);
    }

    [Fact]
    public async Task HandleRequestAsync_GetOrder_WithMissingOrderId_ReturnsBadRequest()
    {
        // Arrange
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrder
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.BadRequest, result.ErrorType);
        Assert.Contains(BillbeeQueryParameters.OrderId, result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_GetProduct_WithValidProductId_ReturnsSuccess()
    {
        // Arrange
        var expectedProduct = new Product { Id = "123" };
        _mockProductService.Setup(x => x.GetProductAsync("123"))
            .ReturnsAsync(expectedProduct);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetProduct,
            QueryParameters = { { BillbeeQueryParameters.ProductId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedProduct, result.Data);
    }

    [Fact]
    public async Task HandleRequestAsync_GetProducts_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var expectedProducts = new ProductResponse
        {
            Products = new List<Product> { new Product { Id = "123" } },
            Paging = new PagingInfo { Page = 1, TotalCount = 1 }
        };
        _mockProductService.Setup(x => x.GetProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedProducts);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetProducts
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedProducts, result.Data);
    }

    [Fact]
    public async Task HandleRequestAsync_GetShippingProfiles_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var expectedProfiles = new List<ShippingProfile>
        {
            new ShippingProfile { Id = "1", Name = "Standard" }
        };
        _mockShippingService.Setup(x => x.GetShippingProfilesAsync())
            .ReturnsAsync(expectedProfiles);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetShippingProfiles
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedProfiles, result.Data);
    }

    [Fact]
    public async Task HandleRequestAsync_AckOrder_WithValidOrderId_ReturnsSuccess()
    {
        // Arrange
        _mockOrderService.Setup(x => x.AckOrderAsync("123"))
            .Returns(Task.CompletedTask);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Post,
            Action = BillbeeActions.AckOrder,
            FormParameters = { { BillbeeQueryParameters.OrderId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("OK", result.Data);
    }

    [Fact]
    public async Task HandleRequestAsync_AckOrder_WithMissingOrderId_ReturnsBadRequest()
    {
        // Arrange
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Post,
            Action = BillbeeActions.AckOrder
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.BadRequest, result.ErrorType);
        Assert.Contains(BillbeeQueryParameters.OrderId, result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_SetOrderState_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        _mockOrderService.Setup(x => x.SetOrderStateAsync(It.IsAny<SetOrderStateRequest>()))
            .Returns(Task.CompletedTask);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Post,
            Action = BillbeeActions.SetOrderState,
            FormParameters = 
            {
                { BillbeeQueryParameters.OrderId, "123" },
                { BillbeeQueryParameters.NewStateId, "1" },
                { BillbeeQueryParameters.Comment, "Test comment" }
            }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("OK", result.Data);
        _mockOrderService.Verify(x => x.SetOrderStateAsync(It.Is<SetOrderStateRequest>(r => 
            r.OrderId == "123" && 
            r.Comment == "Test comment")), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_SetStock_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        _mockStockService.Setup(x => x.SetStockAsync(It.IsAny<SetStockRequest>()))
            .Returns(Task.CompletedTask);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Post,
            Action = BillbeeActions.SetStock,
            FormParameters = 
            {
                { BillbeeQueryParameters.ProductId, "123" },
                { BillbeeQueryParameters.AvailableStock, "50" }
            }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("OK", result.Data);
        _mockStockService.Verify(x => x.SetStockAsync(It.Is<SetStockRequest>(r => 
            r.ProductId == "123" && 
            r.AvailableStock == 50)), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullOrderService_ReturnsNotFound()
    {
        // Arrange
        _service.OrderService = null;
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("Order service not implemented", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullProductService_ReturnsNotFound()
    {
        // Arrange
        _service.ProductService = null;
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetProduct,
            QueryParameters = { { BillbeeQueryParameters.ProductId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("Product service not implemented", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullStockService_ReturnsNotFound()
    {
        // Arrange
        _service.StockService = null;
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Post,
            Action = BillbeeActions.SetStock,
            FormParameters = { { BillbeeQueryParameters.ProductId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("Stock service not implemented", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullShippingService_ReturnsNotFound()
    {
        // Arrange
        _service.ShippingService = null;
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetShippingProfiles
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("Shipping service not implemented", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithOrderServiceException_ReturnsInternalServerError()
    {
        // Arrange
        _mockOrderService.Setup(x => x.GetOrdersAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.InternalServerError, result.ErrorType);
        Assert.Contains("Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_GetOrder_WithOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockOrderService.Setup(x => x.GetOrderAsync("123"))
            .ReturnsAsync((Order?)null);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrder,
            QueryParameters = { { BillbeeQueryParameters.OrderId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("Order with ID 123 not found", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_GetProduct_WithProductNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockProductService.Setup(x => x.GetProductAsync("123"))
            .ReturnsAsync((Product?)null);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetProduct,
            QueryParameters = { { BillbeeQueryParameters.ProductId, "123" } }
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
        Assert.Contains("Product with ID 123 not found", result.ErrorMessage);
    }

    [Theory]
    [InlineData(BillbeeActions.GetOrders, BillbeeMethods.Post)]
    [InlineData(BillbeeActions.GetOrder, BillbeeMethods.Post)]
    [InlineData(BillbeeActions.GetProduct, BillbeeMethods.Post)]
    [InlineData(BillbeeActions.GetProducts, BillbeeMethods.Post)]
    [InlineData(BillbeeActions.GetShippingProfiles, BillbeeMethods.Post)]
    [InlineData(BillbeeActions.AckOrder, BillbeeMethods.Get)]
    [InlineData(BillbeeActions.SetOrderState, BillbeeMethods.Get)]
    [InlineData(BillbeeActions.SetStock, BillbeeMethods.Get)]
    public async Task HandleRequestAsync_WithWrongHttpMethod_ReturnsBadRequest(string action, string method)
    {
        // Arrange
        var request = new BillbeeRequest
        {
            Method = method,
            Action = action
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.BadRequest, result.ErrorType);
        Assert.Contains($"Invalid action '{action}' for method '{method}'", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNoApiKeyConfigured_AllowsAllRequests()
    {
        // Arrange
        _service.ApiKey = null; // No API key configured
        var expectedOrders = new OrderResponse();
        _mockOrderService.Setup(x => x.GetOrdersAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedOrders);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders,
            Key = null // No key provided
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNoBasicAuthConfigured_AllowsAllRequests()
    {
        // Arrange
        _service.BasicAuthCredentials = (null, null); // No basic auth configured
        var expectedOrders = new OrderResponse();
        _mockOrderService.Setup(x => x.GetOrdersAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedOrders);

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders,
            AuthorizationHeader = null // No auth header provided
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }
}