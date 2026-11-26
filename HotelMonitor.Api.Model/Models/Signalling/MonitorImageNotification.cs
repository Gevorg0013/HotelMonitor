namespace HotelMonitor.Api.Models;

/// <summary>One image in the NewMonitorImages SignalR event (metadata only, never the bytes).</summary>
public sealed record MonitorImageNotification(
    int Id, int SequenceNumber, string FileName, string ContentType, DateTime CreatedAt);
