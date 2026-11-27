using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelMonitor.Core.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToEntityNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityMonitorManager",
                table: "IdentityMonitorManager");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "MonitorImage");

            migrationBuilder.RenameTable(
                name: "IdentityMonitorManager",
                newName: "AuthUser");

            migrationBuilder.RenameIndex(
                name: "IX_Images_SequenceNumber",
                table: "MonitorImage",
                newName: "IX_MonitorImage_SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityMonitorManager_Username",
                table: "AuthUser",
                newName: "IX_AuthUser_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonitorImage",
                table: "MonitorImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthUser",
                table: "AuthUser",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MonitorImage",
                table: "MonitorImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthUser",
                table: "AuthUser");

            migrationBuilder.RenameTable(
                name: "MonitorImage",
                newName: "Images");

            migrationBuilder.RenameTable(
                name: "AuthUser",
                newName: "IdentityMonitorManager");

            migrationBuilder.RenameIndex(
                name: "IX_MonitorImage_SequenceNumber",
                table: "Images",
                newName: "IX_Images_SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "IX_AuthUser_Username",
                table: "IdentityMonitorManager",
                newName: "IX_IdentityMonitorManager_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityMonitorManager",
                table: "IdentityMonitorManager",
                column: "Id");
        }
    }
}
