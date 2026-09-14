namespace RMitra.BuildingBlocks.Responses;

public class ApiError
{
    public string Code { get; init; } = "ERROR";
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<object> Details { get; init; } = [];
}

public class ApiResponse
{
    public bool Success { get; init; }
    public object? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse Ok(object? data = null) =>
        new() { Success = true, Data = data };

    public static ApiResponse Fail(string code, string message, IReadOnlyList<object>? details = null) =>
        new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details ?? []
            }
        };
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
