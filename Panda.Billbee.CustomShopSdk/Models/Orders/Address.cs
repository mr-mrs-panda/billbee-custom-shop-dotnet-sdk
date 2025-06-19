using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents a postal address for invoice or delivery purposes.
/// </summary>
public class Address
{
    /// <summary>
    /// First name of the contact.
    /// </summary>
    [JsonPropertyName("firstname")]
    public string? Firstname { get; set; }
    /// <summary>
    /// Last name of the contact.
    /// </summary>
    [JsonPropertyName("lastname")]
    public string? Lastname { get; set; }
    /// <summary>
    /// Street name of the address.
    /// </summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }
    /// <summary>
    /// House number of the address.
    /// </summary>
    [JsonPropertyName("housenumber")]
    public string? Housenumber { get; set; }
    /// <summary>
    /// Additional address line (e.g., apartment or suite).
    /// </summary>
    [JsonPropertyName("address2")]
    public string? Address2 { get; set; }
    /// <summary>
    /// Postal code of the address.
    /// </summary>
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }
    /// <summary>
    /// City of the address.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }
    /// <summary>
    /// ISO country code (e.g., DE).
    /// </summary>
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
    /// <summary>
    /// Company name if applicable.
    /// </summary>
    [JsonPropertyName("company")]
    public string? Company { get; set; }
    /// <summary>
    /// State or region of the address.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
}