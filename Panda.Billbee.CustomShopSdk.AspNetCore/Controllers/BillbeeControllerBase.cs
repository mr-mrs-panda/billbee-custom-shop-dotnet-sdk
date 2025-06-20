using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Panda.Billbee.CustomShopSdk.AspNetCore.Helpers;
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Models;

namespace Panda.Billbee.CustomShopSdk.AspNetCore.Controllers;

/// <summary>
/// Base controller class that provides common Billbee API endpoint functionality.
/// Inherit from this class to quickly create Billbee-compatible controllers with minimal code.
/// </summary>
public abstract class BillbeeControllerBase(ILogger<BillbeeControllerBase>? logger) : ControllerBase
{
    /// <summary>
    /// Gets the Billbee service instance for handling requests.
    /// </summary>
    protected abstract IBillbeeCustomShopService BillbeeService { get; }

    /// <summary>
    /// Notifies the system of an error that occurred while processing a Billbee request.
    /// </summary>
    /// <param name="serviceResult">The result of the service operation, containing error details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task NotifyErrorAsync(ServiceResult serviceResult)
    {
        var errorMessage = serviceResult.GetErrorMessage();
        logger?.LogError(serviceResult.Exception, errorMessage);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles GET requests to the Billbee API endpoint.
    /// </summary>
    /// <param name="action">The action parameter from the query string</param>
    /// <param name="key">The key parameter from the query string (optional)</param>
    /// <returns>An IActionResult with the appropriate HTTP response</returns>
    [HttpGet]
    public virtual async Task<IActionResult> HandleGetRequest([FromQuery] string action, [FromQuery] string? key = null)
    {
        var serviceResult = await BillbeeControllerHelper.HandleGetRequestAsync(BillbeeService, Request, action, key);
        await NotifyErrorIfNeededAsync(serviceResult);
        
        var actionResult = BillbeeControllerHelper.ConvertToActionResult(serviceResult);
        SetWwwAuthenticateHeaderIfUnauthorized(actionResult);
        return actionResult;
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
        var serviceResult = await BillbeeControllerHelper.HandlePostRequestAsync(BillbeeService, Request, action, key);
        await NotifyErrorIfNeededAsync(serviceResult);
        
        var actionResult = BillbeeControllerHelper.ConvertToActionResult(serviceResult);
        SetWwwAuthenticateHeaderIfUnauthorized(actionResult);
        return actionResult;
    }

    /// <summary>
    /// Sets the WWW-Authenticate header for 401 Unauthorized responses.
    /// </summary>
    /// <param name="result">The action result to check</param>
    private void SetWwwAuthenticateHeaderIfUnauthorized(IActionResult result)
    {
        if (result is UnauthorizedObjectResult)
        {
            Response.Headers.WWWAuthenticate = "Basic realm=\"Billbee API\"";
        }
    }
    
    /// <summary>
    /// Notifies the system of an error if the service result indicates failure.
    /// </summary>
    /// <param name="serviceResult">The result of the service operation, containing error details.</param>
    private async Task NotifyErrorIfNeededAsync(ServiceResult serviceResult)
    {
        if (!serviceResult.IsSuccess)
        {
            await NotifyErrorAsync(serviceResult);
        }
    }
}