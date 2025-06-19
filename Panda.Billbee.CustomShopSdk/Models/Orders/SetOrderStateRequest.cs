using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents the request parameters for changing an order status via the SetOrderState endpoint.
/// </summary>
public class SetOrderStateRequest
{
    /// <summary>
    /// Internal ID of the order whose status is to be changed.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }
    /// <summary>
    /// New status code to set for the order.
    /// </summary>
    [JsonPropertyName("new_state_id")]
    public OrderStatus? NewStateId { get; set; }
    /// <summary>
    /// Optional comment to add to the order history or send to the customer.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
    /// <summary>
    /// Shipping carrier code or name for the shipment.
    /// </summary>
    [JsonPropertyName("shipping_carrier")]
    public string? ShippingCarrier { get; set; }
    /// <summary>
    /// Tracking number or code for the shipment.
    /// </summary>
    [JsonPropertyName("tracking_code")]
    public string? TrackingCode { get; set; }
    /// <summary>
    /// URL for tracking the shipment status.
    /// </summary>
    [JsonPropertyName("tracking_url")]
    public string? TrackingUrl { get; set; }
}