using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudgeWeb.Data.Migrations
{
    public partial class AddVotesRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Problem",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubmissionCount",
                table: "Problem",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<int>(
                name: "AcceptCount",
                table: "Problem",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AddColumn<int>(
                name: "Downvote",
                table: "Problem",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AddColumn<int>(
                name: "Upvote",
                table: "Problem",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MessageStatus",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MessageId",
                table: "MessageStatus",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Message",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Judge",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProblemId",
                table: "Judge",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Judge",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GroupJoin",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupJoin",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupContestConfig",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContestId",
                table: "GroupContestConfig",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Group",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ContestRegister",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContestId",
                table: "ContestRegister",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubmissionCount",
                table: "ContestProblemConfig",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<int>(
                name: "ProblemId",
                table: "ContestProblemConfig",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContestId",
                table: "ContestProblemConfig",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AcceptCount",
                table: "ContestProblemConfig",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Contest",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Downvote",
                table: "Contest",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AddColumn<int>(
                name: "Upvote",
                table: "Contest",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AddColumn<int>(
                name: "AcceptedCount",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContinuousSignedIn",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignedIn",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SubmissionCount",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VotesRecord",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProblemId = table.Column<int>(nullable: true),
                    ContestId = table.Column<int>(nullable: true),
                    GroupId = table.Column<int>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    VoteTime = table.Column<DateTime>(nullable: false),
                    VoteType = table.Column<int>(nullable: false, defaultValueSql: "1"),
                    Title = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotesRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotesRecord_Contest_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotesRecord_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotesRecord_Problem_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotesRecord_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Problem_UserId",
                table: "Problem",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageStatus_UserId",
                table: "MessageStatus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_UserId",
                table: "Message",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Judge_UserId",
                table: "Judge",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupJoin_UserId",
                table: "GroupJoin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Group_UserId",
                table: "Group",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestRegister_UserId",
                table: "ContestRegister",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contest_UserId",
                table: "Contest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_ContestId",
                table: "VotesRecord",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_GroupId",
                table: "VotesRecord",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_ProblemId",
                table: "VotesRecord",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_UserId",
                table: "VotesRecord",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contest_AspNetUsers_UserId",
                table: "Contest",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestRegister_AspNetUsers_UserId",
                table: "ContestRegister",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Group_AspNetUsers_UserId",
                table: "Group",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoin_AspNetUsers_UserId",
                table: "GroupJoin",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_AspNetUsers_UserId",
                table: "Judge",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_AspNetUsers_UserId",
                table: "Message",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageStatus_AspNetUsers_UserId",
                table: "MessageStatus",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Problem_AspNetUsers_UserId",
                table: "Problem",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contest_AspNetUsers_UserId",
                table: "Contest");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestRegister_AspNetUsers_UserId",
                table: "ContestRegister");

            migrationBuilder.DropForeignKey(
                name: "FK_Group_AspNetUsers_UserId",
                table: "Group");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoin_AspNetUsers_UserId",
                table: "GroupJoin");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_AspNetUsers_UserId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_AspNetUsers_UserId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageStatus_AspNetUsers_UserId",
                table: "MessageStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_Problem_AspNetUsers_UserId",
                table: "Problem");

            migrationBuilder.DropTable(
                name: "VotesRecord");

            migrationBuilder.DropIndex(
                name: "IX_Problem_UserId",
                table: "Problem");

            migrationBuilder.DropIndex(
                name: "IX_MessageStatus_UserId",
                table: "MessageStatus");

            migrationBuilder.DropIndex(
                name: "IX_Message_UserId",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_Judge_UserId",
                table: "Judge");

            migrationBuilder.DropIndex(
                name: "IX_GroupJoin_UserId",
                table: "GroupJoin");

            migrationBuilder.DropIndex(
                name: "IX_Group_UserId",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_ContestRegister_UserId",
                table: "ContestRegister");

            migrationBuilder.DropIndex(
                name: "IX_Contest_UserId",
                table: "Contest");

            migrationBuilder.DropColumn(
                name: "Downvote",
                table: "Problem");

            migrationBuilder.DropColumn(
                name: "Upvote",
                table: "Problem");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Judge");

            migrationBuilder.DropColumn(
                name: "Downvote",
                table: "Contest");

            migrationBuilder.DropColumn(
                name: "Upvote",
                table: "Contest");

            migrationBuilder.DropColumn(
                name: "AcceptedCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContinuousSignedIn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastSignedIn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubmissionCount",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Problem",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubmissionCount",
                table: "Problem",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<int>(
                name: "AcceptCount",
                table: "Problem",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MessageStatus",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MessageId",
                table: "MessageStatus",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Message",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Judge",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProblemId",
                table: "Judge",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GroupJoin",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupJoin",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupContestConfig",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "ContestId",
                table: "GroupContestConfig",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Group",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ContestRegister",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContestId",
                table: "ContestRegister",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "SubmissionCount",
                table: "ContestProblemConfig",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<int>(
                name: "ProblemId",
                table: "ContestProblemConfig",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "ContestId",
                table: "ContestProblemConfig",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "AcceptCount",
                table: "ContestProblemConfig",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Contest",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
