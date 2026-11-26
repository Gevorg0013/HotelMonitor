using System.Net;
using System.Text.Json.Serialization;

namespace HotelMonitor.Common.Models;

public class BaseResponse(HttpStatusCode statusCode = HttpStatusCode.OK, string message = "")
{
    [JsonIgnore]
    public int StatusCode { get; } = (int)statusCode;

    [JsonIgnore]
    public string Message { get; } = message;
}
