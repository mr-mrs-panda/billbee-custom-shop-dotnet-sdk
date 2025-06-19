namespace Panda.Billbee.CustomShopSdk.Models;

/// <summary>
/// Represents an incoming Billbee API request, including HTTP method, action, API key, and parameters.
/// </summary>
public class BillbeeRequest
{
    /// <summary>
    /// HTTP method of the request (e.g., GET or POST).
    /// </summary>
    public string Method { get; set; } = string.Empty;
    /// <summary>
    /// The action parameter specifying the requested API method (e.g., GetOrders, AckOrder).
    /// </summary>
    public string? Action { get; set; }
    /// <summary>
    /// Encrypted API key parameter used for request authentication.
    /// </summary>
    public string? Key { get; set; }
    /// <summary>
    /// Authorization header for Basic Authentication.
    /// </summary>
    public string? AuthorizationHeader { get; set; }
    /// <summary>
    /// Parsed query string parameters from the request URL.
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    /// <summary>
    /// Parsed form body parameters for POST requests.
    /// </summary>
    public Dictionary<string, string> FormParameters { get; set; } = new();

    /// <summary>
    /// Retrieves the value of a query parameter by key.
    /// </summary>
    /// <param name="key">Parameter name.</param>
    /// <returns>Parameter value or null if not present.</returns>
    public string? GetQueryParameter(string key) => QueryParameters.TryGetValue(key, out var value) ? value : null;
    /// <summary>
    /// Retrieves the value of a form parameter by key.
    /// </summary>
    /// <param name="key">Parameter name.</param>
    /// <returns>Parameter value or null if not present.</returns>
    public string? GetFormParameter(string key) => FormParameters.TryGetValue(key, out var value) ? value : null;
    /// <summary>
    /// Retrieves the value of a form or query parameter by key, preferring form parameters.
    /// </summary>
    /// <param name="key">Parameter name.</param>
    /// <returns>Parameter value or null if not present.</returns>
    public string? GetParameter(string key) => GetFormParameter(key) ?? GetQueryParameter(key);
}