using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Shipping;

/// <summary>
/// Represents a shipping profile entry as returned by the Billbee Custom Shop API.
/// </summary>
public class ShippingProfile
{
    /// <summary>
    /// Identifier of the shipping profile (alphanumeric string).
    /// </summary>
    [JsonPropertyName("Id")]
    public string? Id { get; set; }
    /// <summary>
    /// Display name of the shipping profile.
    /// </summary>
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}