using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudgeWeb.Data.Migrations
{
    public partial class AddAnnouncementSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageContent_Message_MessageId",
                table: "MessageContent");

            migrationBuilder.AlterColumn<int>(
                name: "MessageId",
                table: "MessageContent",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    PublishTime = table.Column<DateTime>(nullable: false),
                    Hidden = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcement_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_UserId",
                table: "Announcement",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageContent_Message_MessageId",
                table: "MessageContent",
                column: "MessageId",
                principalTable: "Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageContent_Message_MessageId",
                table: "MessageContent");

            migrationBuilder.DropTable(
                name: "Announcement");

            migrationBuilder.AlterColumn<int>(
                name: "MessageId",
                table: "MessageContent",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageContent_Message_MessageId",
                table: "MessageContent",
                column: "MessageId",
                principalTable: "Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
