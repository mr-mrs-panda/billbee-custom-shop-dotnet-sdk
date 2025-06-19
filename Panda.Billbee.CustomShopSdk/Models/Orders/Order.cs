using System.Text.Json.Serialization;

namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Represents an order as returned by the Billbee Custom Shop API.
/// </summary>
public class Order
{
    /// <summary>
    /// Internal ID of the order.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }
    /// <summary>
    /// Display order number; may differ from OrderId.
    /// </summary>
    [JsonPropertyName("order_number")]
    public string? OrderNumber { get; set; }
    /// <summary>
    /// ISO currency code for the order (e.g., EUR).
    /// </summary>
    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; set; }
    /// <summary>
    /// Customer nickname or identifier.
    /// </summary>
    [JsonPropertyName("nick_name")]
    public string? NickName { get; set; }
    /// <summary>
    /// Shipping cost for the order (gross amount).
    /// </summary>
    [JsonPropertyName("ship_cost")]
    public decimal ShipCost { get; set; }
    /// <summary>
    /// Invoice address of the customer.
    /// </summary>
    [JsonPropertyName("invoice_address")]
    public Address? InvoiceAddress { get; set; }
    /// <summary>
    /// Delivery address of the customer.
    /// </summary>
    [JsonPropertyName("delivery_address")]
    public Address? DeliveryAddress { get; set; }
    /// <summary>
    /// Date and time when the order was placed.
    /// </summary>
    [JsonPropertyName("order_date")]
    public DateTime? OrderDate { get; set; }
    /// <summary>
    /// Customer email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    /// <summary>
    /// Primary phone number of the customer.
    /// </summary>
    [JsonPropertyName("phone1")]
    public string? Phone1 { get; set; }
    /// <summary>
    /// Date and time when payment was received.
    /// </summary>
    [JsonPropertyName("pay_date")]
    public DateTime? PayDate { get; set; }
    /// <summary>
    /// Date and time when the order was shipped.
    /// </summary>
    [JsonPropertyName("ship_date")]
    public DateTime? ShipDate { get; set; }

    /// <summary>
    /// Payment method code as defined by Billbee API.
    /// </summary>
    [JsonPropertyName("payment_method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;
    /// <summary>
    /// Current status code of the order as defined by Billbee API.
    /// </summary>
    [JsonPropertyName("order_status_id")]
    public OrderStatus? OrderStatusId { get; set; }
    /// <summary>
    /// List of products included in the order.
    /// </summary>
    [JsonPropertyName("order_products")]
    public List<OrderProduct>? OrderProducts { get; set; }
    /// <summary>
    /// History entries and comments related to the order.
    /// </summary>
    [JsonPropertyName("order_history")]
    public List<OrderHistory>? OrderHistory { get; set; }
    /// <summary>
    /// Internal seller comment to highlight order notes.
    /// </summary>
    [JsonPropertyName("seller_comment")]
    public string? SellerComment { get; set; }
    /// <summary>
    /// Shipping profile identifier for mapping to Billbee shipping products.
    /// </summary>
    [JsonPropertyName("shippingprofile_id")]
    public string? ShippingProfileId { get; set; }
    /// <summary>
    /// VAT identification number for the customer.
    /// </summary>
    [JsonPropertyName("vat_id")]
    public string? VatId { get; set; }
}