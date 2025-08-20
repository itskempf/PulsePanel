using System.Text.Json.Serialization;

namespace PulsePanel.Api.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Success(T data) => new() { Status = "ok", Data = data };
    public static ApiResponse<object> Fail(string code, string message) => new() { Status = "error", Error = new ApiError { Code = code, Message = message } };
}

public class ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "UNKNOWN";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "An unknown error occurred.";
}
