using Microsoft.AspNetCore.Mvc;
using Panda.Billbee.CustomShopSdk.AspNetCore.Helpers;
using Panda.Billbee.CustomShopSdk.Interfaces;

namespace Panda.Billbee.CustomShopSdk.AspNetCore.Controllers;

/// <summary>
/// Base controller class that provides common Billbee API endpoint functionality.
/// Inherit from this class to quickly create Billbee-compatible controllers with minimal code.
/// </summary>
public abstract class BillbeeControllerBase : ControllerBase
{
    /// <summary>
    /// Gets the Billbee service instance for handling requests.
    /// </summary>
    protected abstract IBillbeeCustomShopService BillbeeService { get; }

    /// <summary>
    /// Handles GET requests to the Billbee API endpoint.
    /// </summary>
    /// <param name="action">The action parameter from the query string</param>
    /// <param name="key">The key parameter from the query string (optional)</param>
    /// <returns>An IActionResult with the appropriate HTTP response</returns>
    [HttpGet]
    public virtual async Task<IActionResult> HandleGetRequest([FromQuery] string action, [FromQuery] string? key = null)
    {
        var result = await BillbeeControllerHelper.HandleGetRequestAsync(BillbeeService, Request, action, key);
        
        // Add WWW-Authenticate header for 401 responses
        if (result is UnauthorizedObjectResult)
        {
            Response.Headers.WWWAuthenticate = "Basic realm=\"Billbee API\"";
        }
        
        return result;
    }

    /// <summary>
    /// Handles POST requests to the Billbee API endpoint.
    /// </summary>
    /// <param name="action">The action parameter from the query string</param>
    /// <param name="key">The key parameter from the query string (optional)</param>
    /// <returns>An IActionResult with the appropriate HTTP response</returns>
    [HttpPost]
    public virtual async Task<IActionResult> HandlePostRequest([FromQuery] string action, [FromQuery] string? key = null)
    {
        var result = await BillbeeControllerHelper.HandlePostRequestAsync(BillbeeService, Request, action, key);
        
        // Add WWW-Authenticate header for 401 responses
        if (result is UnauthorizedObjectResult)
        {
            Response.Headers.WWWAuthenticate = "Basic realm=\"Billbee API\"";
        }
        
        return result;
    }
}