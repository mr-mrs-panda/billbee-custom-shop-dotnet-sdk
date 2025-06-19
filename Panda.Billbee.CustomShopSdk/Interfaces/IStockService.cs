using Panda.Billbee.CustomShopSdk.Models.Products;

namespace Panda.Billbee.CustomShopSdk.Interfaces;

/// <summary>
/// Defines operations for updating inventory in a Billbee Custom Shop integration.
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Updates the available stock for a product.
    /// Corresponds to the HTTP POST Action=SetStock endpoint.
    /// </summary>
    /// <param name="request">Details of the stock update including ProductId and AvailableStock.</param>
    /// <returns>A task representing the stock update operation.</returns>
    Task SetStockAsync(SetStockRequest request);
}