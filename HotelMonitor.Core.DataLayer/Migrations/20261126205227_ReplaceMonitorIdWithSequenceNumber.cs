using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelMonitor.Core.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMonitorIdWithSequenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageTable_Auth_MonitorId",
                table: "ImageTable");

            migrationBuilder.DropIndex(
                name: "IX_ImageTable_MonitorId_Id",
                table: "ImageTable");

            migrationBuilder.DropColumn(
                name: "MonitorId",
                table: "ImageTable");

            migrationBuilder.CreateSequence<int>(
                name: "ImageSequenceNumbers");

            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "ImageTable",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR [ImageSequenceNumbers]");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTable_SequenceNumber",
                table: "ImageTable",
                column: "SequenceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageTable_SequenceNumber",
                table: "ImageTable");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "ImageTable");

            migrationBuilder.DropSequence(
                name: "ImageSequenceNumbers");

            migrationBuilder.AddColumn<int>(
                name: "MonitorId",
                table: "ImageTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTable_MonitorId_Id",
                table: "ImageTable",
                columns: new[] { "MonitorId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ImageTable_Auth_MonitorId",
                table: "ImageTable",
                column: "MonitorId",
                principalTable: "Auth",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
