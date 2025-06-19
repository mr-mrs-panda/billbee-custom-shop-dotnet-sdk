using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents paging information for list responses.
/// </summary>
public class PagingInfo
{
    /// <summary>
    /// Current page number.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }
    /// <summary>
    /// Total number of records available.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; set; }
    /// <summary>
    /// Total number of pages available.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; set; }
}