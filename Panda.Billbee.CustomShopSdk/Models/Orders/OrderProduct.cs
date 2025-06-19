using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents a product entry within an order.
/// </summary>
public class OrderProduct
{
    /// <summary>
    /// Discount percentage applied to the product.
    /// </summary>
    [JsonPropertyName("discount_percent")]
    public decimal DiscountPercent { get; set; }
    /// <summary>
    /// Quantity of the product ordered.
    /// </summary>
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
    /// <summary>
    /// Gross unit price of the product.
    /// </summary>
    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }
    /// <summary>
    /// Internal ID of the product.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }
    /// <summary>
    /// Name or title of the product.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    /// <summary>
    /// Stock-keeping unit (SKU) or product number.
    /// </summary>
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }
    /// <summary>
    /// Tax rate applied to the product (e.g., 19.00 for 19%).
    /// </summary>
    [JsonPropertyName("tax_rate")]
    public decimal TaxRate { get; set; }
    /// <summary>
    /// List of options selected for the product (e.g., color, size).
    /// </summary>
    [JsonPropertyName("options")]
    public List<OrderProductOption>? Options { get; set; }
}