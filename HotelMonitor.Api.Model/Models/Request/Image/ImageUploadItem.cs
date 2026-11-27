using Microsoft.AspNetCore.Http;

namespace HotelMonitor.Api.Models
{
    public record ImageUploadItem
    {
        public IFormFile File { get; set; }

        public int SequenceId { get; set; }
    }
}
