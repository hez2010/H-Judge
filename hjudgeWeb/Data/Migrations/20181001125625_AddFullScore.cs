using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudgeWeb.Data.Migrations
{
    public partial class AddFullScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "FullScore",
                table: "Judge",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullScore",
                table: "Judge");
        }
    }
}
