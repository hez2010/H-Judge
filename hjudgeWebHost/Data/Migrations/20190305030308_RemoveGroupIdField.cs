using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudgeWebHost.Data.Migrations
{
    public partial class RemoveGroupIdField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VotesRecord_Group_GroupId",
                table: "VotesRecord");

            migrationBuilder.DropIndex(
                name: "IX_VotesRecord_GroupId",
                table: "VotesRecord");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "VotesRecord");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "VotesRecord",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VotesRecord_Group_GroupId",
                table: "VotesRecord",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_GroupId",
                table: "VotesRecord",
                column: "GroupId");
        }
    }
}
