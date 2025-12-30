namespace Contracting.Shared.Common;

public class GenericResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }

    public static GenericResponse SuccessResult(string? message = null) 
        => new GenericResponse { Success = true, Message = message ?? "Operation completed successfully" };
    
    public static GenericResponse FailureResult(string message, params string[] errors) 
        => new GenericResponse { Success = false, Message = message, Errors = errors };
}

public class GenericResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string[]? Errors { get; set; }

    public static GenericResponse<T> SuccessResult(T data, string? message = null) 
        => new GenericResponse<T> { Success = true, Data = data, Message = message ?? "Operation completed successfully" };
    
    public static GenericResponse<T> FailureResult(string message, params string[] errors) 
        => new GenericResponse<T> { Success = false, Message = message, Errors = errors };
}
