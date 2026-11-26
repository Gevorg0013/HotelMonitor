using Microsoft.AspNetCore.Http;

namespace HotelMonitor.Api.Models;

public sealed class ImageUploadRequest
{
    public List<IFormFile> Files { get; set; } = [];

    public List<int> SequenceNumbers { get; set; } = [];
}
