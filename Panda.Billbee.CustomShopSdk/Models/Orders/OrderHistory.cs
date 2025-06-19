using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents a history entry or comment for an order.
/// </summary>
public class OrderHistory
{
    /// <summary>
    /// Date and time when the history entry was added.
    /// </summary>
    [JsonPropertyName("date_added")]
    public DateTime? DateAdded { get; set; }
    /// <summary>
    /// Name or source of the comment (e.g., customer or system).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    /// <summary>
    /// Text content of the history entry or comment.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
    /// <summary>
    /// Indicates whether the entry was added by the customer.
    /// </summary>
    [JsonPropertyName("from_customer")]
    public bool? FromCustomer { get; set; }
}