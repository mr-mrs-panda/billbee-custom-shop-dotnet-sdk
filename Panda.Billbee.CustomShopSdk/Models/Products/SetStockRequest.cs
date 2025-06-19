using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Products;

/// <summary>
/// Represents the request parameters for updating product stock via the SetStock endpoint.
/// </summary>
public class SetStockRequest
{
    /// <summary>
    /// Internal ID of the product whose stock is to be updated.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }
    /// <summary>
    /// New available stock quantity for the product.
    /// </summary>
    [JsonPropertyName("available_stock")]
    public decimal? AvailableStock { get; set; }
}