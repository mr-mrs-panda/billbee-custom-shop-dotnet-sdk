using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Products;

/// <summary>
/// Represents a product as returned by the Billbee Custom Shop API.
/// </summary>
public class Product
{
    /// <summary>
    /// Internal ID of the product.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    /// <summary>
    /// Full description of the product (can include HTML).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    /// <summary>
    /// Short description of the product (can include HTML).
    /// </summary>
    [JsonPropertyName("shortdescription")]
    public string? ShortDescription { get; set; }
    /// <summary>
    /// Basic or key attributes of the product.
    /// </summary>
    [JsonPropertyName("basic_attributes")]
    public string? BasicAttributes { get; set; }
    /// <summary>
    /// Title or short name of the product.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    /// <summary>
    /// List of images for the product.
    /// </summary>
    [JsonPropertyName("images")]
    public List<ProductImage>? Images { get; set; }
    /// <summary>
    /// Gross price of the product.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    /// <summary>
    /// Available stock quantity of the product.
    /// </summary>
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }
    /// <summary>
    /// Stock-keeping unit (SKU) or product number.
    /// </summary>
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }
    /// <summary>
    /// Weight of the product in kilograms.
    /// </summary>
    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }
    /// <summary>
    /// VAT rate applied to the product (e.g., 19.00 for 19%).
    /// </summary>
    [JsonPropertyName("vat_rate")]
    public decimal? VatRate { get; set; }
}