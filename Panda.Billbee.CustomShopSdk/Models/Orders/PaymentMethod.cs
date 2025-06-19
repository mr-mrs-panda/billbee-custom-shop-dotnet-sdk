namespace Panda.Billbee.CustomShopSdk.Models.Orders;

/// <summary>
/// Payment method codes as defined by the Billbee Custom Shop API.
/// </summary>
public enum PaymentMethod
{
    BankTransfer = 1,
    CashOnDelivery = 2,
    PayPal = 3,
    Cash = 4,
    Voucher = 6,
    Sofort = 19,
    Other = 22,
    DirectDebit = 23,
    Klarna = 25,
    Invoice = 26,
    CreditCard = 31,
    Maestro = 32,
    AmazonPayments = 44,
    Prepayment = 59,
    AmazonMarketplace = 61,
    AmazonPaymentsAdvanced = 62,
    Stripe = 63,
    SumUp = 67,
    Installment = 73,
    EtsyPayments = 97,
    Klarna2 = 98,
    Ebay = 104
}