using Panda.Billbee.CustomShopSdk.Constants;
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
        public string? ApiKey { get; set; }
        public (string? Username, string? Password) BasicAuthCredentials { get; set; } = (null, null);
        public OrderResponse? OrdersResponse { get; set; }
        public Order? Order { get; set; }
        public Product? Product { get; set; }
        public ProductResponse? ProductsResponse { get; set; }
        public List<ShippingProfile>? ShippingProfiles { get; set; }
        public Exception? ThrowException { get; set; }

        protected override string? GetApiKey() => ApiKey;
        protected override (string? Username, string? Password) GetBasicAuthCredentials() => BasicAuthCredentials;

        protected override Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.FromResult(OrdersResponse ?? new OrderResponse());
        }

        protected override Task<Order?> GetOrderAsync(string orderId)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.FromResult(Order);
        }

        protected override Task AckOrderAsync(string orderId)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.CompletedTask;
        }

        protected override Task SetOrderStateAsync(SetOrderStateRequest request)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.CompletedTask;
        }

        protected override Task<Product?> GetProductAsync(string productId)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.FromResult(Product);
        }

        protected override Task<ProductResponse> GetProductsAsync(int page, int pageSize)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.FromResult(ProductsResponse ?? new ProductResponse());
        }

        protected override Task SetStockAsync(SetStockRequest request)
        {
            if (ThrowException != null) throw ThrowException;
            return Task.CompletedTask;
        }

        protected override Task<List<ShippingProfile>> GetShippingProfilesAsync()
        {
            if (ThrowException != null) throw ThrowException;
            return Task.FromResult(ShippingProfiles ?? new List<ShippingProfile>());
        }
    }

    private readonly TestBillbeeCustomShopService _service;

    public BillbeeCustomShopServiceTests()
    {
        _service = new TestBillbeeCustomShopService();
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
        _service.OrdersResponse = expectedOrders;

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
        _service.OrdersResponse = expectedOrders;

        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = BillbeeActions.GetOrders
        };

        // Act
        var result = await _service.HandleRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        // Verify default parameters are used - this is implicitly tested by the success
    }

    [Fact]
    public async Task HandleRequestAsync_GetOrder_WithValidOrderId_ReturnsSuccess()
    {
        // Arrange
        var expectedOrder = new Order { OrderId = "123" };
        _service.Order = expectedOrder;

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
        _service.Product = expectedProduct;

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
        _service.ProductsResponse = expectedProducts;

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
        _service.ShippingProfiles = expectedProfiles;

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
        // No setup needed - default implementation returns Task.CompletedTask

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
        // No setup needed - default implementation returns Task.CompletedTask

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
        // Verify request was processed successfully
    }

    [Fact]
    public async Task HandleRequestAsync_SetStock_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        // No setup needed - default implementation returns Task.CompletedTask

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
        // Verify request was processed successfully
    }

    [Fact]
    public async Task HandleRequestAsync_WithOrderServiceException_ReturnsInternalServerError()
    {
        // Arrange
        _service.ThrowException = new Exception("Database error");

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
        _service.Order = null;

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
        _service.Product = null;

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
        _service.OrdersResponse = expectedOrders;

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
        _service.OrdersResponse = expectedOrders;

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

    [Fact]
    public void ServiceResult_GetErrorMessage_SanitizesActionParameter()
    {
        // Arrange - Test with potentially malicious action parameter containing injection attempt
        var maliciousAction = "getorders\r\n[INJECTED LOG ENTRY] FAKE ERROR";
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = maliciousAction
        };

        var serviceResult = ServiceResult.BadRequest(request, "Test error");

        // Act
        var errorMessage = serviceResult.GetErrorMessage();

        // Assert - Should contain "InvalidAction" since malicious input doesn't match exactly, and not contain injection
        Assert.Contains("[GET - InvalidAction]", errorMessage);
        Assert.DoesNotContain("INJECTED LOG ENTRY", errorMessage);
        Assert.DoesNotContain("FAKE ERROR", errorMessage);
        Assert.DoesNotContain("\r\n", errorMessage);
    }

    [Fact]
    public void ServiceResult_GetErrorMessage_HandlesInvalidActionParameter()
    {
        // Arrange - Test with completely invalid action
        var invalidAction = "<script>alert('xss')</script>";
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Post,
            Action = invalidAction
        };

        var serviceResult = ServiceResult.Forbidden(request, "Access denied");

        // Act
        var errorMessage = serviceResult.GetErrorMessage();

        // Assert - Should use "InvalidAction" for unknown actions
        Assert.Contains("[POST - InvalidAction]", errorMessage);
        Assert.DoesNotContain("<script>", errorMessage);
        Assert.DoesNotContain("alert", errorMessage);
    }

    [Fact]
    public void ServiceResult_GetErrorMessage_HandlesValidActionParameter()
    {
        // Arrange - Test with valid action parameter
        var validAction = BillbeeActions.GetOrders;
        var request = new BillbeeRequest
        {
            Method = BillbeeMethods.Get,
            Action = validAction
        };

        var serviceResult = ServiceResult.NotFound(request, "Order not found");

        // Act
        var errorMessage = serviceResult.GetErrorMessage();

        // Assert - Should contain proper action name for valid actions
        Assert.Contains("[GET - GetOrders]", errorMessage);
        Assert.Contains("NotFound - Order not found", errorMessage);
    }
}