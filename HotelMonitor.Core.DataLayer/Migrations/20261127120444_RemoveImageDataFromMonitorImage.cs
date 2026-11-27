using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelMonitor.Core.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImageDataFromMonitorImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "MonitorImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "MonitorImage",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
