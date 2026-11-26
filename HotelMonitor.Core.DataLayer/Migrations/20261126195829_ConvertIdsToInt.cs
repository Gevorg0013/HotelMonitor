using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelMonitor.Core.DataLayer.Migrations
{
    /// <summary>
    /// Switches Auth.Id and ImageTable.Id from uniqueidentifier to INT IDENTITY and removes ImageTable.SequenceNumber.
    /// SQL Server cannot convert uniqueidentifier to int or add IDENTITY to an existing column, so both HotelMonitor
    /// tables are dropped and recreated. Existing HotelMonitor users and images are deleted; no other table is touched.
    /// </summary>
    public partial class ConvertIdsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageTable");

            migrationBuilder.DropTable(
                name: "Auth");

            migrationBuilder.CreateTable(
                name: "Auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitorId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageTable_Auth_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Auth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auth_Username",
                table: "Auth",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTable_MonitorId_Id",
                table: "ImageTable",
                columns: new[] { "MonitorId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageTable");

            migrationBuilder.DropTable(
                name: "Auth");

            migrationBuilder.CreateTable(
                name: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageTable_Auth_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Auth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auth_Username",
                table: "Auth",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTable_MonitorId_SequenceNumber",
                table: "ImageTable",
                columns: new[] { "MonitorId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageTable_SequenceNumber",
                table: "ImageTable",
                column: "SequenceNumber",
                unique: true);
        }
    }
}
