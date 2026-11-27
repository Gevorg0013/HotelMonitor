namespace HotelMonitor.Api.Models;

public sealed class MonitorImageNotification
{
    public int Id { get; set; }
    public int SequenceNumber { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string WebPath { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}