using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelMonitor.Core.DataLayer;

public class HotelMonitorDbContext(DbContextOptions<HotelMonitorDbContext> options) : DbContext(options), IGenericDbContext
{
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<MonitorImage> MonitorImages => Set<MonitorImage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthUser>(e =>
        {
            // VTM already owns dbo.Users in the shared database.
            e.ToTable("IdentityMonitorManager");
            e.Property(x => x.Id).UseIdentityColumn();
            e.Property(x => x.Username).HasMaxLength(100);
            e.Property(x => x.PasswordHash).HasMaxLength(512);
            e.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<MonitorImage>(e =>
        {
            e.ToTable("Images");
            e.Property(x => x.Id).UseIdentityColumn();
            e.Property(x => x.FileName).HasMaxLength(255);
            e.Property(x => x.ContentType).HasMaxLength(50);
            // byte[] maps to varbinary(max) by default on SQL Server.
            // The client supplies SequenceNumber; the unique index rejects a number that is already used.
            e.HasIndex(x => x.SequenceNumber).IsUnique();
        });
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
}
