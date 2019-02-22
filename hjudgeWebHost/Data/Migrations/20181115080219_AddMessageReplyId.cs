using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudgeWebHost.Data.Migrations
{
    public partial class AddMessageReplyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplyId",
                table: "Message",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReplyId",
                table: "Discussion",
                nullable: false,
                defaultValueSql: "0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "Discussion");
        }
    }
}
