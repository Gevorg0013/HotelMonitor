namespace HotelMonitor.Api.Models;

public sealed record ImageUploadRequest
{
    public List<ImageUploadItem> Images { get; set; } = [];

}
