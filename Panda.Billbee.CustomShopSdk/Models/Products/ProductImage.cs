using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Products;

/// <summary>
/// Represents an image entry for a product.
/// </summary>
public class ProductImage
{
    /// <summary>
    /// URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    /// <summary>
    /// Indicates whether this image is the default product image.
    /// </summary>
    [JsonPropertyName("isDefault")]
    public bool? IsDefault { get; set; }
    /// <summary>
    /// Position index of the image in the product gallery.
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }
}