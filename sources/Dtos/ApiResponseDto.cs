
namespace SwiftUI_backend_demo.sources.Dtos;

/// <summary>
/// Generic API response wrapper for returning data payloads.
/// </summary>
/// <typeparam name="T">The type of the business data payload.</typeparam>
public class ApiResponseDto<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponseDto<T> Success(T data, string message = "Success")
    {
        return new ApiResponseDto<T>
        {
            StatusCode = 0,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponseDto<T> Fail(int code, string message)
    {
        return new ApiResponseDto<T>
        {
            StatusCode = code,
            Message = message,
            Data = default,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Non-generic API response wrapper for operations returning no data payload.
/// </summary>
public class ApiResponseDto : ApiResponseDto<object>
{
    public static ApiResponseDto Success(string message = "Success")
    {
        return new ApiResponseDto
        {
            StatusCode = 0,
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow
        };
    }
}