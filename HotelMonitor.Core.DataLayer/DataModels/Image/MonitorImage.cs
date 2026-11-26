using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelMonitor.Core.DataLayer.Entities;

[Table("MonitorImage")]
public class MonitorImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int SequenceNumber { get; set; }
    public string FileName { get; set; } = "";
    public byte[] ImageData { get; set; } = [];
    public string ContentType { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
