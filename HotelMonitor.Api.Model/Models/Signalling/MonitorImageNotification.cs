namespace HotelMonitor.Api.Models;

public sealed record MonitorImageNotification(int Id, int SequenceNumber, string FileName, string ContentType, DateTime CreatedAt);
