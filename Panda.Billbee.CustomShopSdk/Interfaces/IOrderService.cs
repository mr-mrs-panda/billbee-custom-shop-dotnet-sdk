using Panda.Billbee.CustomShopSdk.Models.Orders;

namespace Panda.Billbee.CustomShopSdk.Interfaces;

/// <summary>
/// Defines operations for handling orders in a Billbee Custom Shop integration.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Retrieves new and changed orders since the specified start date.
    /// Corresponds to the HTTP GET Action=GetOrders endpoint.
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD) from which new or changed orders should be returned.</param>
    /// <param name="page">Page number of the data retrieval; default is 1.</param>
    /// <param name="pageSize">Maximum number of records per page; default is 100.</param>
    /// <returns>An <see cref="OrderResponse"/> containing paging info and orders.</returns>
    Task<OrderResponse> GetOrdersAsync(DateTime startDate, int page, int pageSize);
    /// <summary>
    /// Acknowledges receipt of an order to prevent its re-transmission.
    /// Corresponds to the HTTP POST Action=AckOrder endpoint.
    /// </summary>
    /// <param name="orderId">Internal ID of the order to acknowledge.</param>
    /// <returns>A task representing the acknowledgment operation.</returns>
    Task AckOrderAsync(string orderId);
    /// <summary>
    /// Retrieves a single order by its internal ID.
    /// Corresponds to the HTTP GET Action=GetOrder endpoint.
    /// </summary>
    /// <param name="orderId">Internal ID of the order to retrieve.</param>
    /// <returns>The <see cref="Order"/> or null if not found.</returns>
    Task<Order?> GetOrderAsync(string orderId);
    /// <summary>
    /// Changes the status of an order in the shop system.
    /// Corresponds to the HTTP POST Action=SetOrderState endpoint.
    /// </summary>
    /// <param name="request">Details of the status change including OrderId, NewStateId, Comment, ShippingCarrier, TrackingCode, and TrackingUrl.</param>
    /// <returns>A task representing the status update operation.</returns>
    Task SetOrderStateAsync(SetOrderStateRequest request);
}