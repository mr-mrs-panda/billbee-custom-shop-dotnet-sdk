using Panda.Billbee.CustomShopSdk.Constants;
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Models;
using Panda.Billbee.CustomShopSdk.Models.Orders;
using Panda.Billbee.CustomShopSdk.Models.Products;
using Panda.Billbee.CustomShopSdk.Models.Shipping;
using Panda.Billbee.CustomShopSdk.Security;

namespace Panda.Billbee.CustomShopSdk.Services;

/// <summary>
/// Base service class that implements the Billbee Custom Shop API routing and request handling logic.
/// </summary>
public abstract class BillbeeCustomShopService : IBillbeeCustomShopService
{
    /// <summary>
    /// Provides the <see cref="IOrderService"/> implementation for order-related operations.
    /// </summary>
    protected abstract IOrderService? GetOrderService();

    /// <summary>
    /// Provides the <see cref="IProductService"/> implementation for product-related operations.
    /// </summary>
    protected abstract IProductService? GetProductService();

    /// <summary>
    /// Provides the <see cref="IStockService"/> implementation for stock update operations.
    /// </summary>
    protected abstract IStockService? GetStockService();

    /// <summary>
    /// Provides the <see cref="IShippingService"/> implementation for shipping profile operations.
    /// </summary>
    protected abstract IShippingService? GetShippingService();

    /// <summary>
    /// Retrieves the configured API key for validating incoming requests; return null or empty to disable authentication.
    /// </summary>
    protected abstract string? GetApiKey();

    /// <summary>
    /// Returns the credentials for Basic Authentication (username, password).
    /// Returns (null, null) to disable Basic Authentication.
    /// </summary>
    /// <returns>A tuple with username and password.</returns>
    protected virtual (string? Username, string? Password) GetBasicAuthCredentials() => (null, null);

    /// <summary>
    /// Processes an incoming <see cref="BillbeeRequest"/> and routes to the appropriate endpoint handler based on action and HTTP method.
    /// </summary>
    /// <param name="request">Parsed request data including action, parameters, and method.</param>
    /// <returns>A <see cref="ServiceResult"/> indicating the HTTP response status and payload.</returns>
    public async Task<ServiceResult> HandleRequestAsync(BillbeeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Action))
                return ServiceResult.BadRequest("Action parameter is required");

            if (!ValidateApiKey(request.Key) || !ValidateBasicAuth(request.AuthorizationHeader))
                return ServiceResult.Unauthorized();

            return request.Action.ToLower() switch
            {
                BillbeeActions.GetOrders when request.Method.Equals(BillbeeMethods.Get,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleGetOrdersAsync(request),
                BillbeeActions.GetOrder when request.Method.Equals(BillbeeMethods.Get,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleGetOrderAsync(request),
                BillbeeActions.GetProduct when request.Method.Equals(BillbeeMethods.Get,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleGetProductAsync(request),
                BillbeeActions.GetProducts when request.Method.Equals(BillbeeMethods.Get,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleGetProductsAsync(request),
                BillbeeActions.GetShippingProfiles when request.Method.Equals(BillbeeMethods.Get,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleGetShippingProfilesAsync(),
                BillbeeActions.AckOrder when request.Method.Equals(BillbeeMethods.Post,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleAckOrderAsync(request),
                BillbeeActions.SetOrderState when request.Method.Equals(BillbeeMethods.Post,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleSetOrderStateAsync(request),
                BillbeeActions.SetStock when request.Method.Equals(BillbeeMethods.Post,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleSetStockAsync(request),
                _ => ServiceResult.BadRequest($"Invalid action '{request.Action}' for method '{request.Method}'")
            };
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult> HandleGetOrdersAsync(BillbeeRequest request)
    {
        var startDateStr = request.GetQueryParameter(BillbeeQueryParameters.StartDate);
        if (string.IsNullOrEmpty(startDateStr) || !DateTime.TryParse(startDateStr, out var startDate))
        {
            startDate = DateTime.UtcNow.AddYears(-10);
        }

        int.TryParse(request.GetQueryParameter(BillbeeQueryParameters.Page), out var page);
        int.TryParse(request.GetQueryParameter(BillbeeQueryParameters.PageSize), out var pageSize);

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 100;

        var result = await GetOrdersAsync(startDate, page, pageSize);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleGetOrderAsync(BillbeeRequest request)
    {
        var orderId = request.GetQueryParameter(BillbeeQueryParameters.OrderId);
        if (string.IsNullOrEmpty(orderId))
            return ServiceResult.BadRequest($"{BillbeeQueryParameters.OrderId} is required");

        var result = await GetOrderAsync(orderId);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleGetProductAsync(BillbeeRequest request)
    {
        var productId = request.GetQueryParameter(BillbeeQueryParameters.ProductId);
        if (string.IsNullOrEmpty(productId))
            return ServiceResult.BadRequest($"{BillbeeQueryParameters.ProductId} is required");

        var result = await GetProductAsync(productId);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleGetProductsAsync(BillbeeRequest request)
    {
        int.TryParse(request.GetQueryParameter(BillbeeQueryParameters.Page), out var page);
        int.TryParse(request.GetQueryParameter(BillbeeQueryParameters.PageSize), out var pageSize);

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 100;

        var result = await GetProductsAsync(page, pageSize);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleGetShippingProfilesAsync()
    {
        var result = await GetShippingProfilesAsync();
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleAckOrderAsync(BillbeeRequest request)
    {
        var orderId = request.GetFormParameter(BillbeeQueryParameters.OrderId);
        if (string.IsNullOrEmpty(orderId))
            return ServiceResult.BadRequest($"{BillbeeQueryParameters.OrderId} is required");

        var result = await AckOrderAsync(orderId);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleSetOrderStateAsync(BillbeeRequest request)
    {
        var setOrderStateRequest = new SetOrderStateRequest
        {
            OrderId = request.GetFormParameter(BillbeeQueryParameters.OrderId),
            NewStateId =
                Enum.TryParse<OrderStatus>(request.GetFormParameter(BillbeeQueryParameters.NewStateId), out var status)
                    ? status
                    : null,
            Comment = request.GetFormParameter(BillbeeQueryParameters.Comment),
            ShippingCarrier = request.GetFormParameter(BillbeeQueryParameters.ShippingCarrier),
            TrackingCode = request.GetFormParameter(BillbeeQueryParameters.TrackingCode),
            TrackingUrl = request.GetFormParameter(BillbeeQueryParameters.TrackingUrl)
        };

        var result = await SetOrderStateAsync(setOrderStateRequest);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private async Task<ServiceResult> HandleSetStockAsync(BillbeeRequest request)
    {
        var setStockRequest = new SetStockRequest
        {
            ProductId = request.GetFormParameter(BillbeeQueryParameters.ProductId),
            AvailableStock =
                decimal.TryParse(request.GetFormParameter(BillbeeQueryParameters.AvailableStock), out var stock)
                    ? stock
                    : null
        };

        var result = await SetStockAsync(setStockRequest);
        return result.IsSuccess
            ? ServiceResult.Success(result.Data!)
            : ServiceResult.CreateFromResult(result);
    }

    private bool ValidateApiKey(string? receivedKey)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
            return true;

        return !string.IsNullOrEmpty(receivedKey) && ApiKeyAuthenticator.Validate(apiKey, receivedKey);
    }

    private bool ValidateBasicAuth(string? authHeader)
    {
        var (username, password) = GetBasicAuthCredentials();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return true;

        return ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);
    }

    private async Task<ServiceResult<OrderResponse>> GetOrdersAsync(DateTime startDate, int page = 1,
        int pageSize = 100)
    {
        try
        {
            var orderService = GetOrderService();
            if (orderService == null)
                return ServiceResult<OrderResponse>.NotFound("Order service not implemented");

            var result = await orderService.GetOrdersAsync(startDate, page, pageSize);
            return ServiceResult<OrderResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderResponse>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<Order>> GetOrderAsync(string orderId)
    {
        try
        {
            if (string.IsNullOrEmpty(orderId))
                return ServiceResult<Order>.BadRequest("OrderId is required");

            var orderService = GetOrderService();
            if (orderService == null)
                return ServiceResult<Order>.NotFound("Order service not implemented");

            var result = await orderService.GetOrderAsync(orderId);
            return result == null
                ? ServiceResult<Order>.NotFound($"Order with ID {orderId} not found")
                : ServiceResult<Order>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<Order>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<string>> AckOrderAsync(string orderId)
    {
        try
        {
            if (string.IsNullOrEmpty(orderId))
                return ServiceResult<string>.BadRequest("OrderId is required");

            var orderService = GetOrderService();
            if (orderService == null)
                return ServiceResult<string>.NotFound("Order service not implemented");

            await orderService.AckOrderAsync(orderId);
            return ServiceResult<string>.Success("OK");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<string>> SetOrderStateAsync(SetOrderStateRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.OrderId))
                return ServiceResult<string>.BadRequest("Valid SetOrderStateRequest with OrderId is required");

            var orderService = GetOrderService();
            if (orderService == null)
                return ServiceResult<string>.NotFound("Order service not implemented");

            await orderService.SetOrderStateAsync(request);
            return ServiceResult<string>.Success("OK");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<Product>> GetProductAsync(string productId)
    {
        try
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<Product>.BadRequest("ProductId is required");

            var productService = GetProductService();
            if (productService == null)
                return ServiceResult<Product>.NotFound("Product service not implemented");

            var result = await productService.GetProductAsync(productId);
            if (result == null)
                return ServiceResult<Product>.NotFound($"Product with ID {productId} not found");

            return ServiceResult<Product>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<Product>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<ProductResponse>> GetProductsAsync(int page = 1, int pageSize = 100)
    {
        try
        {
            var productService = GetProductService();
            if (productService == null)
                return ServiceResult<ProductResponse>.NotFound("Product service not implemented");

            var result = await productService.GetProductsAsync(page, pageSize);
            return ServiceResult<ProductResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductResponse>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<string>> SetStockAsync(SetStockRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProductId))
                return ServiceResult<string>.BadRequest("Valid SetStockRequest with ProductId is required");

            var stockService = GetStockService();
            if (stockService == null)
                return ServiceResult<string>.NotFound("Stock service not implemented");

            await stockService.SetStockAsync(request);
            return ServiceResult<string>.Success("OK");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.InternalServerError(ex.Message);
        }
    }

    private async Task<ServiceResult<List<ShippingProfile>>> GetShippingProfilesAsync()
    {
        try
        {
            var shippingService = GetShippingService();
            if (shippingService == null)
                return ServiceResult<List<ShippingProfile>>.NotFound("Shipping service not implemented");

            var result = await shippingService.GetShippingProfilesAsync();
            return ServiceResult<List<ShippingProfile>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ShippingProfile>>.InternalServerError(ex.Message);
        }
    }
}