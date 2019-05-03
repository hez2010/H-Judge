using Microsoft.EntityFrameworkCore.Migrations;

namespace hjudge.WebHost.Data.Migrations
{
    public partial class AddDefaultDeleteMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContestProblemConfig_Contest_ContestId",
                table: "ContestProblemConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestProblemConfig_Problem_ProblemId",
                table: "ContestProblemConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestRegister_Contest_ContestId",
                table: "ContestRegister");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupContestConfig_Contest_ContestId",
                table: "GroupContestConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupContestConfig_Group_GroupId",
                table: "GroupContestConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoin_Group_GroupId",
                table: "GroupJoin");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_Contest_ContestId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_Group_GroupId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_Problem_ProblemId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageStatus_Message_MessageId",
                table: "MessageStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_ContestProblemConfig_Contest_ContestId",
                table: "ContestProblemConfig",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestProblemConfig_Problem_ProblemId",
                table: "ContestProblemConfig",
                column: "ProblemId",
                principalTable: "Problem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestRegister_Contest_ContestId",
                table: "ContestRegister",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupContestConfig_Contest_ContestId",
                table: "GroupContestConfig",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupContestConfig_Group_GroupId",
                table: "GroupContestConfig",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoin_Group_GroupId",
                table: "GroupJoin",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_Contest_ContestId",
                table: "Judge",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_Group_GroupId",
                table: "Judge",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_Problem_ProblemId",
                table: "Judge",
                column: "ProblemId",
                principalTable: "Problem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageStatus_Message_MessageId",
                table: "MessageStatus",
                column: "MessageId",
                principalTable: "Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContestProblemConfig_Contest_ContestId",
                table: "ContestProblemConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestProblemConfig_Problem_ProblemId",
                table: "ContestProblemConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_ContestRegister_Contest_ContestId",
                table: "ContestRegister");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupContestConfig_Contest_ContestId",
                table: "GroupContestConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupContestConfig_Group_GroupId",
                table: "GroupContestConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupJoin_Group_GroupId",
                table: "GroupJoin");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_Contest_ContestId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_Group_GroupId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_Judge_Problem_ProblemId",
                table: "Judge");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageStatus_Message_MessageId",
                table: "MessageStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_ContestProblemConfig_Contest_ContestId",
                table: "ContestProblemConfig",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestProblemConfig_Problem_ProblemId",
                table: "ContestProblemConfig",
                column: "ProblemId",
                principalTable: "Problem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContestRegister_Contest_ContestId",
                table: "ContestRegister",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupContestConfig_Contest_ContestId",
                table: "GroupContestConfig",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupContestConfig_Group_GroupId",
                table: "GroupContestConfig",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupJoin_Group_GroupId",
                table: "GroupJoin",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_Contest_ContestId",
                table: "Judge",
                column: "ContestId",
                principalTable: "Contest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_Group_GroupId",
                table: "Judge",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Judge_Problem_ProblemId",
                table: "Judge",
                column: "ProblemId",
                principalTable: "Problem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageStatus_Message_MessageId",
                table: "MessageStatus",
                column: "MessageId",
                principalTable: "Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
