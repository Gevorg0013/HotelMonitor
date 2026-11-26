namespace HotelMonitor.Api.Models;

public sealed record MonitorImageResponse(
    int Id, int SequenceNumber, string FileName,
    string ContentType, long FileSize, DateTime CreatedAt);
