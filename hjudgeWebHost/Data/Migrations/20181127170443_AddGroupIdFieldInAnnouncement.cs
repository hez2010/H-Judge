using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudgeWebHost.Data.Migrations
{
    public partial class AddGroupIdFieldInAnnouncement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Announcement",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_GroupId",
                table: "Announcement",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_Group_GroupId",
                table: "Announcement",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_Group_GroupId",
                table: "Announcement");

            migrationBuilder.DropIndex(
                name: "IX_Announcement_GroupId",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Announcement");
        }
    }
}
