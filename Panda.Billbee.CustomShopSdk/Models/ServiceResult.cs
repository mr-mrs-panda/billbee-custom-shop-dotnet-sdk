using Panda.Billbee.CustomShopSdk.Constants;

namespace Panda.Billbee.CustomShopSdk.Models;

/// <summary>
/// Represents the result of handling a Billbee API request, including success flag, returned data, or error information.
/// </summary>
public class ServiceResult
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// The data returned on success.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// The error message returned on failure.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The exception that occurred during processing, if any.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// The type of error for failure scenarios.
    /// </summary>
    public ServiceErrorType ErrorType { get; set; }

    /// <summary>
    /// The original Billbee request that was processed, if available.
    /// </summary>
    public BillbeeRequest? Request { get; set; }

    public static ServiceResult Success(BillbeeRequest request, object? data = null)
    {
        return new ServiceResult
        {
            IsSuccess = true,
            Data = data,
            ErrorType = ServiceErrorType.None,
            Request = request
        };
    }

    public static ServiceResult Unauthorized(BillbeeRequest request, string message = "Unauthorized")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.Unauthorized,
            Request = request
        };
    }

    public static ServiceResult NotFound(BillbeeRequest request, string message = "Not Found")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.NotFound,
            Request = request
        };
    }

    public static ServiceResult BadRequest(BillbeeRequest request, string message = "Bad Request")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.BadRequest,
            Request = request
        };
    }

    public static ServiceResult Forbidden(BillbeeRequest request, string message = "Forbidden")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.Forbidden,
            Request = request
        };
    }

    public static ServiceResult InternalServerError(BillbeeRequest request, string message = "Internal Server Error")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.InternalServerError,
            Request = request
        };
    }

    public static ServiceResult InternalServerError(BillbeeRequest request, Exception exception)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = exception.Message,
            Exception = exception,
            ErrorType = ServiceErrorType.InternalServerError,
            Request = request
        };
    }

    public string? GetErrorMessage()
    {
        if (IsSuccess)
        {
            return null;
        }

        var requestPrefix = Request == null ? string.Empty : $"[{Request.Method} - {GetSafeActionName(Request.Action)}] | ";
        var errorMessage = Exception?.Message ?? ErrorMessage;
        var message = $"{requestPrefix}{ErrorType} - {errorMessage}.";
        return message;
    }

    /// <summary>
    /// Sanitizes the action parameter for safe logging by only allowing known valid actions.
    /// This prevents log injection attacks from user-controlled input.
    /// </summary>
    /// <param name="action">The action parameter from user input</param>
    /// <returns>A sanitized action name safe for logging</returns>
    private static string GetSafeActionName(string? action)
    {
        if (string.IsNullOrEmpty(action))
            return "Unknown";

        // Only allow known valid actions to prevent log injection
        return action.ToLowerInvariant() switch
        {
            BillbeeActions.GetOrders => "GetOrders",
            BillbeeActions.GetOrder => "GetOrder", 
            BillbeeActions.GetProduct => "GetProduct",
            BillbeeActions.GetProducts => "GetProducts",
            BillbeeActions.GetShippingProfiles => "GetShippingProfiles",
            BillbeeActions.AckOrder => "AckOrder",
            BillbeeActions.SetOrderState => "SetOrderState",
            BillbeeActions.SetStock => "SetStock",
            _ => "InvalidAction"
        };
    }
}

/// <summary>
/// Error types for <see cref="ServiceResult"/> to indicate the corresponding HTTP response.
/// </summary>
public enum ServiceErrorType
{
    None,
    Unauthorized,
    NotFound,
    BadRequest,
    Forbidden,
    InternalServerError
}