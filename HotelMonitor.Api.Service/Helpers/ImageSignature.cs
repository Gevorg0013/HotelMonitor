namespace HotelMonitor.Api.Service.Helpers;

/// <summary>Identifies an image from its leading bytes; the client's Content-Type and extension are not trusted.</summary>
public static class ImageSignature
{
    public static string? Detect(ReadOnlySpan<byte> data)
    {
        ReadOnlySpan<byte> jpeg = [0xFF, 0xD8, 0xFF];
        ReadOnlySpan<byte> png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        if (data.StartsWith(jpeg))
            return "image/jpeg";

        if (data.StartsWith(png))
            return "image/png";

        if (data.Length >= 12 && data.StartsWith("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";

        return null;
    }
}
