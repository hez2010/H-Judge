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
                    FileId = table.Column<string>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    OriginalFileName = table.Column<string>(nullable: false),
                    ContentType = table.Column<string>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    FileSize = table.Column<long>(nullable: false)
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
