using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Products;

/// <summary>
/// Response for a GetProducts request, containing paging information and a list of products.
/// </summary>
public class ProductResponse
{
    /// <summary>
    /// Paging information for the products list.
    /// </summary>
    [JsonPropertyName("paging")]
    public Panda.Billbee.CustomShopSdk.Models.Orders.PagingInfo? Paging { get; set; }
    /// <summary>
    /// List of products returned by the API.
    /// </summary>
    [JsonPropertyName("products")]
    public List<Product>? Products { get; set; }
}