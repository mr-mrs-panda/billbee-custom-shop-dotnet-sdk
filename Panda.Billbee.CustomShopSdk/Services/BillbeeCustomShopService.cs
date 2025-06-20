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
    /// Retrieves new and changed orders since the specified start date.
    /// Corresponds to the HTTP GET Action=GetOrders endpoint.
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD) from which new or changed orders should be returned.</param>
    /// <param name="page">Page number of the data retrieval; default is 1.</param>
    /// <param name="pageSize">Maximum number of records per page; default is 100.</param>
    /// <returns>An <see cref="OrderResponse"/> containing paging info and orders.</returns>
    protected abstract Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize);

    /// <summary>
    /// Acknowledges receipt of an order to prevent its re-transmission.
    /// Corresponds to the HTTP POST Action=AckOrder endpoint.
    /// </summary>
    /// <param name="orderId">Internal ID of the order to acknowledge.</param>
    /// <returns>A task representing the acknowledgment operation.</returns>
    protected abstract Task AckOrderAsync(string orderId);

    /// <summary>
    /// Retrieves a single order by its internal ID.
    /// Corresponds to the HTTP GET Action=GetOrder endpoint.
    /// </summary>
    /// <param name="orderId">Internal ID of the order to retrieve.</param>
    /// <returns>The <see cref="Order"/> or null if not found.</returns>
    protected abstract Task<Order?> GetOrderAsync(string orderId);

    /// <summary>
    /// Changes the status of an order in the shop system.
    /// Corresponds to the HTTP POST Action=SetOrderState endpoint.
    /// </summary>
    /// <param name="request">Details of the status change including OrderId, NewStateId, Comment, ShippingCarrier, TrackingCode, and TrackingUrl.</param>
    /// <returns>A task representing the status update operation.</returns>
    protected abstract Task SetOrderStateAsync(SetOrderStateRequest request);

    /// <summary>
    /// Retrieves a single product by its internal ID.
    /// Corresponds to the HTTP GET Action=GetProduct endpoint.
    /// </summary>
    /// <param name="productId">Internal ID of the product to retrieve.</param>
    /// <returns>The <see cref="Product"/> or null if not found.</returns>
    protected abstract Task<Product?> GetProductAsync(string productId);

    /// <summary>
    /// Retrieves a paged list of products.
    /// Corresponds to the HTTP GET Action=GetProducts endpoint.
    /// </summary>
    /// <param name="page">Page number of the data retrieval; default is 1.</param>
    /// <param name="pageSize">Maximum number of records per page; default is 100.</param>
    /// <returns>A <see cref="ProductResponse"/> containing paging info and products.</returns>
    protected abstract Task<ProductResponse> GetProductsAsync(int page, int pageSize);

    /// <summary>
    /// Updates the available stock for a product.
    /// Corresponds to the HTTP POST Action=SetStock endpoint.
    /// </summary>
    /// <param name="request">Details of the stock update including ProductId and AvailableStock.</param>
    /// <returns>A task representing the stock update operation.</returns>
    protected abstract Task SetStockAsync(SetStockRequest request);

    /// <summary>
    /// Retrieves all shipping profiles from the shop system.
    /// Corresponds to the HTTP GET Action=GetShippingProfiles endpoint.
    /// </summary>
    /// <returns>A list of <see cref="ShippingProfile"/>.</returns>
    protected abstract Task<List<ShippingProfile>> GetShippingProfilesAsync();

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
                return ServiceResult.BadRequest(request, "Action parameter is required");

            if (!ValidateApiKey(request.Key) || !ValidateBasicAuth(request.AuthorizationHeader))
                return ServiceResult.Unauthorized(request);

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
                    StringComparison.CurrentCultureIgnoreCase) => await HandleGetShippingProfilesAsync(request),
                BillbeeActions.AckOrder when request.Method.Equals(BillbeeMethods.Post,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleAckOrderAsync(request),
                BillbeeActions.SetOrderState when request.Method.Equals(BillbeeMethods.Post,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleSetOrderStateAsync(request),
                BillbeeActions.SetStock when request.Method.Equals(BillbeeMethods.Post,
                    StringComparison.CurrentCultureIgnoreCase) => await HandleSetStockAsync(request),
                _ => ServiceResult.BadRequest(request,
                    $"Invalid action '{GetSafeActionName(request.Action)}' for method '{request.Method}'")
            };
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
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

        try
        {
            var result = await GetOrdersAsync(startDate, page, pageSize);
            return ServiceResult.Success(request, result);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
    }

    private async Task<ServiceResult> HandleGetOrderAsync(BillbeeRequest request)
    {
        var orderId = request.GetQueryParameter(BillbeeQueryParameters.OrderId);
        if (string.IsNullOrEmpty(orderId))
            return ServiceResult.BadRequest(request, $"{BillbeeQueryParameters.OrderId} is required");

        try
        {
            var result = await GetOrderAsync(orderId);
            return result == null
                ? ServiceResult.NotFound(request, $"Order with ID {orderId} not found")
                : ServiceResult.Success(request, result);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
    }

    private async Task<ServiceResult> HandleGetProductAsync(BillbeeRequest request)
    {
        var productId = request.GetQueryParameter(BillbeeQueryParameters.ProductId);
        if (string.IsNullOrEmpty(productId))
            return ServiceResult.BadRequest(request, $"{BillbeeQueryParameters.ProductId} is required");

        try
        {
            var result = await GetProductAsync(productId);
            return result == null
                ? ServiceResult.NotFound(request, $"Product with ID {productId} not found")
                : ServiceResult.Success(request, result);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
    }

    private async Task<ServiceResult> HandleGetProductsAsync(BillbeeRequest request)
    {
        int.TryParse(request.GetQueryParameter(BillbeeQueryParameters.Page), out var page);
        int.TryParse(request.GetQueryParameter(BillbeeQueryParameters.PageSize), out var pageSize);

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 100;

        try
        {
            var result = await GetProductsAsync(page, pageSize);
            return ServiceResult.Success(request, result);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
    }

    private async Task<ServiceResult> HandleGetShippingProfilesAsync(BillbeeRequest request)
    {
        try
        {
            var result = await GetShippingProfilesAsync();
            return ServiceResult.Success(request, result);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
    }

    private async Task<ServiceResult> HandleAckOrderAsync(BillbeeRequest request)
    {
        var orderId = request.GetFormParameter(BillbeeQueryParameters.OrderId);
        if (string.IsNullOrEmpty(orderId))
            return ServiceResult.BadRequest(request, $"{BillbeeQueryParameters.OrderId} is required");

        try
        {
            await AckOrderAsync(orderId);
            return ServiceResult.Success(request, "OK");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
    }

    private async Task<ServiceResult> HandleSetOrderStateAsync(BillbeeRequest request)
    {
        var setOrderStateRequest = new SetOrderStateRequest
        {
            OrderId = request.GetFormParameter(BillbeeQueryParameters.OrderId),
            NewStateId = request.GetFormParameter(BillbeeQueryParameters.NewStateId),
            NewStateName = request.GetFormParameter(BillbeeQueryParameters.NewStateName),
            NewStateTypeId =
                Enum.TryParse<OrderStatus>(request.GetFormParameter(BillbeeQueryParameters.NewStateTypeId),
                    out var status)
                    ? status
                    : null,
            Comment = request.GetFormParameter(BillbeeQueryParameters.Comment),
            ShippingCarrier = request.GetFormParameter(BillbeeQueryParameters.ShippingCarrier),
            TrackingCode = request.GetFormParameter(BillbeeQueryParameters.TrackingCode),
            TrackingUrl = request.GetFormParameter(BillbeeQueryParameters.TrackingUrl)
        };

        try
        {
            await SetOrderStateAsync(setOrderStateRequest);
            return ServiceResult.Success(request, "OK");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
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

        try
        {
            await SetStockAsync(setStockRequest);
            return ServiceResult.Success(request, "OK");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(request, ex);
        }
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

    /// <summary>
    /// Sanitizes the action parameter for safe logging by only allowing known valid actions.
    /// This prevents log injection attacks from user-controlled input.
    /// </summary>
    /// <param name="action">The action parameter from user input</param>
    /// <returns>A sanitized action name safe for logging</returns>
    private static string GetSafeActionName(string? action)
    {
        if (string.IsNullOrEmpty(action))
            return "Unknown";

        // Only allow known valid actions to prevent log injection
        return action.ToLowerInvariant() switch
        {
            BillbeeActions.GetOrders => "GetOrders",
            BillbeeActions.GetOrder => "GetOrder", 
            BillbeeActions.GetProduct => "GetProduct",
            BillbeeActions.GetProducts => "GetProducts",
            BillbeeActions.GetShippingProfiles => "GetShippingProfiles",
            BillbeeActions.AckOrder => "AckOrder",
            BillbeeActions.SetOrderState => "SetOrderState",
            BillbeeActions.SetStock => "SetStock",
            _ => "InvalidAction"
        };
    }

}