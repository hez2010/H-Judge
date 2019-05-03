using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudge.WebHost.Data.Migrations
{
    public partial class AdjustMessageStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Message_AspNetUsers_UserId",
                table: "Message");

            migrationBuilder.DropTable(
                name: "MessageStatus");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Message");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Message",
                newName: "ToUserId");

            migrationBuilder.DropColumn(
                name: "Targets",
                table: "Message");

            migrationBuilder.AddColumn<string>(
                name: "FromUserId",
                table: "Message",
                nullable: true,
                maxLength: 450);

            migrationBuilder.RenameIndex(
                name: "IX_Message_UserId",
                table: "Message",
                newName: "IX_Message_ToUserId");

            migrationBuilder.AddColumn<int>(
                name: "ContentId",
                table: "Message",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Message",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MessageContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MessageId = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    UserInfoId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageContent_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageContent_AspNetUsers_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Message_ContentId",
                table: "Message",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageContent_MessageId",
                table: "MessageContent",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageContent_UserInfoId",
                table: "MessageContent",
                column: "UserInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Message_MessageContent_ContentId",
                table: "Message",
                column: "ContentId",
                principalTable: "MessageContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_AspNetUsers_FromUserId",
                table: "Message",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_AspNetUsers_ToUserId",
                table: "Message",
                column: "ToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Message_MessageContent_ContentId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_AspNetUsers_ToUserId",
                table: "Message");

            migrationBuilder.DropTable(
                name: "MessageContent");

            migrationBuilder.DropIndex(
                name: "IX_Message_ContentId",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "ContentId",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Message");

            migrationBuilder.RenameColumn(
                name: "ToUserId",
                table: "Message",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FromUserId",
                table: "Message",
                newName: "Targets");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_AspNetUsers_FromUserId",
                table: "Message");

            migrationBuilder.AlterColumn<string>(
                name: "Targets",
                table: "Message",
                maxLength: null);

            migrationBuilder.RenameIndex(
                name: "IX_Message_ToUserId",
                table: "Message",
                newName: "IX_Message_UserId");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Message",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MessageStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MessageId = table.Column<int>(nullable: false),
                    OperationTime = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true, maxLength: 450)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageStatus_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageStatus_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageStatus_MessageId",
                table: "MessageStatus",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageStatus_UserId",
                table: "MessageStatus",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Message_AspNetUsers_UserId",
                table: "Message",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
