using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Response for a GetOrders request, containing paging information and a list of orders.
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// Paging information for the orders list.
    /// </summary>
    [JsonPropertyName("paging")]
    public PagingInfo? Paging { get; set; }
    /// <summary>
    /// List of orders returned by the API.
    /// </summary>
    [JsonPropertyName("orders")]
    public List<Order>? Orders { get; set; }
}