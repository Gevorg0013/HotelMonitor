using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelMonitor.Core.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesAndClientSequenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageTable",
                table: "ImageTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth",
                table: "Auth");

            migrationBuilder.RenameTable(
                name: "ImageTable",
                newName: "Images");

            migrationBuilder.RenameTable(
                name: "Auth",
                newName: "IdentityMonitorManager");

            migrationBuilder.RenameIndex(
                name: "IX_ImageTable_SequenceNumber",
                table: "Images",
                newName: "IX_Images_SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Username",
                table: "IdentityMonitorManager",
                newName: "IX_IdentityMonitorManager_Username");

            migrationBuilder.AlterColumn<int>(
                name: "SequenceNumber",
                table: "Images",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR [ImageSequenceNumbers]");

            // Only after the default constraint that uses it is gone.
            migrationBuilder.DropSequence(
                name: "ImageSequenceNumbers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityMonitorManager",
                table: "IdentityMonitorManager",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityMonitorManager",
                table: "IdentityMonitorManager");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "ImageTable");

            migrationBuilder.RenameTable(
                name: "IdentityMonitorManager",
                newName: "Auth");

            migrationBuilder.RenameIndex(
                name: "IX_Images_SequenceNumber",
                table: "ImageTable",
                newName: "IX_ImageTable_SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityMonitorManager_Username",
                table: "Auth",
                newName: "IX_Auth_Username");

            migrationBuilder.CreateSequence<int>(
                name: "ImageSequenceNumbers");

            migrationBuilder.AlterColumn<int>(
                name: "SequenceNumber",
                table: "ImageTable",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR [ImageSequenceNumbers]",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageTable",
                table: "ImageTable",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth",
                table: "Auth",
                column: "Id");
        }
    }
}
