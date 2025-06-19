namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Order status codes as defined by the Billbee Custom Shop API.
/// </summary>
public enum OrderStatus
{
    Ordered = 1,
    Confirmed = 2,
    Paid = 3,
    Shipped = 4,
    Complaint = 5,
    Deleted = 6,
    Completed = 7,
    Cancelled = 8,
    Archived = 9,
    Rated = 10,
    FirstReminder = 11,
    SecondReminder = 12,
    Packed = 13,
    Offered = 14,
    PaymentReminder = 15,
    InFulfillment = 16,
    Return = 17
}