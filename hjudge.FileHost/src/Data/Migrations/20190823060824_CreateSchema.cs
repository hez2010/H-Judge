using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudge.FileHost.Data.Migrations
{
    public partial class CreateSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileId = table.Column<string>(),
                    FileName = table.Column<string>(),
                    OriginalFileName = table.Column<string>(),
                    ContentType = table.Column<string>(),
                    LastModified = table.Column<DateTime>(),
                    FileSize = table.Column<long>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
