using Panda.Billbee.CustomShopSdk.Models.Shipping;

namespace Panda.Billbee.CustomShopSdk.Interfaces;

/// <summary>
/// Defines operations for retrieving shipping profiles in a Billbee Custom Shop integration.
/// </summary>
public interface IShippingService
{
    /// <summary>
    /// Retrieves all shipping profiles from the shop system.
    /// Corresponds to the HTTP GET Action=GetShippingProfiles endpoint.
    /// </summary>
    /// <returns>A list of <see cref="ShippingProfile"/>.</returns>
    Task<List<ShippingProfile>> GetShippingProfilesAsync();
}