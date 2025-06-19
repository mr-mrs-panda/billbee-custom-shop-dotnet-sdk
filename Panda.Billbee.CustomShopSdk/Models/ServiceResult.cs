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
    /// The type of error for failure scenarios.
    /// </summary>
    public ServiceErrorType ErrorType { get; set; }

    public static ServiceResult Success(object? data = null)
    {
        return new ServiceResult
        {
            IsSuccess = true,
            Data = data,
            ErrorType = ServiceErrorType.None
        };
    }

    public static ServiceResult Unauthorized(string message = "Unauthorized")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.Unauthorized
        };
    }

    public static ServiceResult NotFound(string message = "Not Found")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.NotFound
        };
    }

    public static ServiceResult BadRequest(string message = "Bad Request")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.BadRequest
        };
    }

    public static ServiceResult Forbidden(string message = "Forbidden")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.Forbidden
        };
    }

    public static ServiceResult InternalServerError(string message = "Internal Server Error")
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.InternalServerError
        };
    }

    public static ServiceResult CreateFromResult<T>(ServiceResult<T> other)
    {
        return new ServiceResult
        {
            IsSuccess = other.IsSuccess,
            Data = other.Data,
            ErrorMessage = other.ErrorMessage,
            ErrorType = other.ErrorType
        };
    }
}

/// <summary>
/// Represents the result of handling a Billbee API request with a typed data payload.
/// </summary>
public class ServiceResult<T>
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// The data returned on success.
    /// </summary>
    public T? Data { get; set; }
    /// <summary>
    /// The error message returned on failure.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// The type of error for failure scenarios.
    /// </summary>
    public ServiceErrorType ErrorType { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            ErrorType = ServiceErrorType.None
        };
    }

    public static ServiceResult<T> Unauthorized(string message = "Unauthorized")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.Unauthorized
        };
    }

    public static ServiceResult<T> NotFound(string message = "Not Found")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.NotFound
        };
    }

    public static ServiceResult<T> BadRequest(string message = "Bad Request")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.BadRequest
        };
    }

    public static ServiceResult<T> Forbidden(string message = "Forbidden")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.Forbidden
        };
    }

    public static ServiceResult<T> InternalServerError(string message = "Internal Server Error")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = ServiceErrorType.InternalServerError
        };
    }

    public static ServiceResult<T> CreateFromResult<TOther>(ServiceResult<TOther> other)
    {
        return new ServiceResult<T>
        {
            IsSuccess = other.IsSuccess,
            ErrorMessage = other.ErrorMessage,
            ErrorType = other.ErrorType
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