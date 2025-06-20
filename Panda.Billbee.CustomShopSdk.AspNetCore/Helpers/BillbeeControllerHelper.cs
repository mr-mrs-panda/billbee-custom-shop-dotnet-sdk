using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Panda.Billbee.CustomShopSdk.Constants;
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Models;

namespace Panda.Billbee.CustomShopSdk.AspNetCore.Helpers;

/// <summary>
/// Helper class to simplify controller integration with Billbee Custom Shop SDK.
/// Provides common functionality for handling HTTP requests and converting them to BillbeeRequest objects.
/// </summary>
public static class BillbeeControllerHelper
{
    /// <summary>
    /// Handles a GET request and processes it through the Billbee service.
    /// </summary>
    /// <param name="service">The Billbee service to handle the request</param>
    /// <param name="httpRequest">The HTTP request from the controller</param>
    /// <param name="action">The action parameter from the query string</param>
    /// <param name="key">The key parameter from the query string (optional)</param>
    /// <returns>An IActionResult with the appropriate HTTP response</returns>
    public static async Task<ServiceResult> HandleGetRequestAsync(
        IBillbeeCustomShopService service,
        HttpRequest httpRequest,
        string action,
        string? key = null)
    {
        var billbeeRequest = CreateBillbeeRequest(BillbeeMethods.Get, action, key, httpRequest);
        return await service.HandleRequestAsync(billbeeRequest);
    }

    /// <summary>
    /// Handles a POST request and processes it through the Billbee service.
    /// </summary>
    /// <param name="service">The Billbee service to handle the request</param>
    /// <param name="httpRequest">The HTTP request from the controller</param>
    /// <param name="action">The action parameter from the query string</param>
    /// <param name="key">The key parameter from the query string (optional)</param>
    /// <returns>A ServiceResult with the appropriate HTTP response</returns>
    public static async Task<ServiceResult> HandlePostRequestAsync(
        IBillbeeCustomShopService service,
        HttpRequest httpRequest,
        string action,
        string? key = null)
    {
        var billbeeRequest = CreateBillbeeRequest(BillbeeMethods.Post, action, key, httpRequest);
        return await service.HandleRequestAsync(billbeeRequest);
    }

    /// <summary>
    /// Creates a BillbeeRequest object from the HTTP request data.
    /// </summary>
    /// <param name="method">The HTTP method (GET or POST)</param>
    /// <param name="action">The action parameter</param>
    /// <param name="key">The key parameter (optional)</param>
    /// <param name="httpRequest">The HTTP request to extract data from</param>
    /// <returns>A configured BillbeeRequest object</returns>
    public static BillbeeRequest CreateBillbeeRequest(string method, string? action, string? key, HttpRequest httpRequest)
    {
        var request = new BillbeeRequest
        {
            Method = method,
            Action = action,
            Key = key,
            AuthorizationHeader = httpRequest.Headers.Authorization.FirstOrDefault()
        };

        // Add query parameters
        foreach (var param in httpRequest.Query)
        {
            request.QueryParameters[param.Key] = param.Value.FirstOrDefault() ?? string.Empty;
        }

        // Add form parameters (for POST requests)
        if (method == BillbeeMethods.Post && httpRequest.HasFormContentType)
        {
            foreach (var param in httpRequest.Form)
            {
                request.FormParameters[param.Key] = param.Value.FirstOrDefault() ?? string.Empty;
            }
        }

        return request;
    }

    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult with correct HTTP status codes.
    /// </summary>
    /// <param name="result">The service result to convert</param>
    /// <returns>An IActionResult with the appropriate status code and content</returns>
    public static IActionResult ConvertToActionResult(ServiceResult result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Data);
        }

        return result.ErrorType switch
        {
            ServiceErrorType.Unauthorized => new UnauthorizedObjectResult(result.ErrorMessage)
            {
                StatusCode = 401
            },
            ServiceErrorType.NotFound => new NotFoundObjectResult(result.ErrorMessage),
            ServiceErrorType.BadRequest => new BadRequestObjectResult(result.ErrorMessage),
            ServiceErrorType.Forbidden => new ObjectResult(result.ErrorMessage) { StatusCode = 403 },
            ServiceErrorType.InternalServerError => new ObjectResult(result.ErrorMessage) { StatusCode = 500 },
            _ => new ObjectResult(result.ErrorMessage) { StatusCode = 500 }
        };
    }

    /// <summary>
    /// Creates an Unauthorized result with the proper WWW-Authenticate header for Basic auth.
    /// </summary>
    /// <param name="message">Optional error message</param>
    /// <returns>An IActionResult for 401 Unauthorized with WWW-Authenticate header</returns>
    public static IActionResult CreateUnauthorizedResult(string? message = null)
    {
        var result = new UnauthorizedObjectResult(message) { StatusCode = 401 };
        
        // Note: Headers should be set in the controller context, not here
        // This method is provided for consistency but headers need to be set by the caller
        return result;
    }
}