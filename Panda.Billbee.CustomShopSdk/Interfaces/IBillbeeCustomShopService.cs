using Panda.Billbee.CustomShopSdk.Models;

namespace Panda.Billbee.CustomShopSdk.Interfaces;

/// <summary>
/// Interface for Billbee Custom Shop API service implementations.
/// </summary>
public interface IBillbeeCustomShopService
{
    /// <summary>
    /// Processes an incoming <see cref="BillbeeRequest"/> and routes to the appropriate endpoint handler based on action and HTTP method.
    /// </summary>
    /// <param name="request">Parsed request data including action, parameters, and method.</param>
    /// <returns>A <see cref="ServiceResult"/> indicating the HTTP response status and payload.</returns>
    Task<ServiceResult> HandleRequestAsync(BillbeeRequest request);
}