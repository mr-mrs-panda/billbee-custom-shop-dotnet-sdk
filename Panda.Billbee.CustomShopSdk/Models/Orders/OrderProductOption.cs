using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents an option (name and value) for a product in an order.
/// </summary>
public class OrderProductOption
{
    /// <summary>
    /// Name of the product option.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    /// <summary>
    /// Value of the product option.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}