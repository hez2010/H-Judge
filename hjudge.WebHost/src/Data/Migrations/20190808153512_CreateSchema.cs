using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace hjudge.WebHost.Data.Migrations
{
    public partial class CreateSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(),
                    TwoFactorEnabled = table.Column<bool>(),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(),
                    AccessFailedCount = table.Column<int>(),
                    Name = table.Column<string>(nullable: true),
                    Coins = table.Column<long>(),
                    Experience = table.Column<long>(),
                    Privilege = table.Column<int>(),
                    Avatar = table.Column<byte[]>(nullable: true),
                    OtherInfo = table.Column<string>(nullable: true),
                    LastSignedIn = table.Column<DateTime>(),
                    ContinuousSignedIn = table.Column<int>(),
                    AcceptedCount = table.Column<int>(),
                    SubmissionCount = table.Column<int>(),
                    MessageReplyCount = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128),
                    ProviderKey = table.Column<string>(maxLength: 128),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(),
                    RoleId = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(),
                    LoginProvider = table.Column<string>(maxLength: 128),
                    Name = table.Column<string>(maxLength: 128),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contest",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(),
                    StartTime = table.Column<DateTime>(),
                    EndTime = table.Column<DateTime>(),
                    Config = table.Column<string>(),
                    Password = table.Column<string>(nullable: true),
                    Description = table.Column<string>(),
                    UserId = table.Column<string>(),
                    SpecifyCompetitors = table.Column<bool>(),
                    Hidden = table.Column<bool>(),
                    AdditionalInfo = table.Column<string>(nullable: true),
                    Upvote = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    Downvote = table.Column<int>(nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contest_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(),
                    Name = table.Column<string>(),
                    Description = table.Column<string>(),
                    CreationTime = table.Column<DateTime>(),
                    IsPrivate = table.Column<bool>(),
                    AdditionalInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Group_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageContent",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<string>(),
                    UserInfoId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageContent_AspNetUsers_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Problem",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(),
                    CreationTime = table.Column<DateTime>(),
                    Level = table.Column<int>(),
                    Config = table.Column<string>(),
                    Type = table.Column<int>(),
                    Description = table.Column<string>(),
                    UserId = table.Column<string>(),
                    Hidden = table.Column<bool>(),
                    AcceptCount = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    SubmissionCount = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    AdditionalInfo = table.Column<string>(nullable: true),
                    Upvote = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    Downvote = table.Column<int>(nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Problem_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestRegister",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContestId = table.Column<int>(),
                    UserId = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestRegister", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestRegister_Contest_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestRegister_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(),
                    Content = table.Column<string>(),
                    UserId = table.Column<string>(),
                    PublishTime = table.Column<DateTime>(),
                    Hidden = table.Column<bool>(),
                    GroupId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcement_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Announcement_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupContestConfig",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(),
                    ContestId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupContestConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupContestConfig_Contest_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupContestConfig_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupJoin",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(),
                    GroupId = table.Column<int>(),
                    JoinTime = table.Column<DateTime>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupJoin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupJoin_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupJoin_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(),
                    ContentId = table.Column<int>(),
                    Type = table.Column<int>(),
                    UserId = table.Column<string>(),
                    SendTime = table.Column<DateTime>(),
                    Status = table.Column<int>(),
                    ReplyId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_MessageContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "MessageContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Message_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestProblemConfig",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContestId = table.Column<int>(),
                    ProblemId = table.Column<int>(),
                    AcceptCount = table.Column<int>(nullable: false, defaultValueSql: "0"),
                    SubmissionCount = table.Column<int>(nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestProblemConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestProblemConfig_Contest_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestProblemConfig_Problem_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Discussion",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(),
                    ProblemId = table.Column<int>(nullable: true),
                    ContestId = table.Column<int>(nullable: true),
                    GroupId = table.Column<int>(nullable: true),
                    SubmitTime = table.Column<DateTime>(),
                    Title = table.Column<string>(),
                    Content = table.Column<string>(),
                    ReplyId = table.Column<int>(nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discussion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discussion_Contest_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discussion_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discussion_Problem_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discussion_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Judge",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(),
                    JudgeTime = table.Column<DateTime>(),
                    ProblemId = table.Column<int>(),
                    ContestId = table.Column<int>(nullable: true),
                    GroupId = table.Column<int>(nullable: true),
                    Content = table.Column<string>(),
                    Result = table.Column<string>(nullable: true),
                    Type = table.Column<int>(),
                    Description = table.Column<string>(nullable: true),
                    ResultType = table.Column<int>(),
                    FullScore = table.Column<float>(),
                    Language = table.Column<string>(nullable: true),
                    Logs = table.Column<string>(nullable: true),
                    AdditionalInfo = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(),
                    JudgeCount = table.Column<int>(nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Judge_Contest_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Judge_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Judge_Problem_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Judge_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VotesRecord",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProblemId = table.Column<int>(nullable: true),
                    ContestId = table.Column<int>(nullable: true),
                    UserId = table.Column<string>(),
                    VoteTime = table.Column<DateTime>(),
                    VoteType = table.Column<int>(nullable: false, defaultValueSql: "1"),
                    Title = table.Column<string>(),
                    Content = table.Column<string>()
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
                name: "IX_Announcement_GroupId",
                table: "Announcement",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_UserId",
                table: "Announcement",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contest_UserId",
                table: "Contest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestProblemConfig_ContestId",
                table: "ContestProblemConfig",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestProblemConfig_ProblemId",
                table: "ContestProblemConfig",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestRegister_ContestId",
                table: "ContestRegister",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestRegister_UserId",
                table: "ContestRegister",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussion_ContestId",
                table: "Discussion",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussion_GroupId",
                table: "Discussion",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussion_ProblemId",
                table: "Discussion",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussion_UserId",
                table: "Discussion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Group_UserId",
                table: "Group",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupContestConfig_ContestId",
                table: "GroupContestConfig",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupContestConfig_GroupId",
                table: "GroupContestConfig",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupJoin_GroupId",
                table: "GroupJoin",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupJoin_UserId",
                table: "GroupJoin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Judge_ContestId",
                table: "Judge",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Judge_GroupId",
                table: "Judge",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Judge_ProblemId",
                table: "Judge",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Judge_UserId",
                table: "Judge",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_ContentId",
                table: "Message",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_UserId",
                table: "Message",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageContent_UserInfoId",
                table: "MessageContent",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Problem_UserId",
                table: "Problem",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_ContestId",
                table: "VotesRecord",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_ProblemId",
                table: "VotesRecord",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_VotesRecord_UserId",
                table: "VotesRecord",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcement");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ContestProblemConfig");

            migrationBuilder.DropTable(
                name: "ContestRegister");

            migrationBuilder.DropTable(
                name: "Discussion");

            migrationBuilder.DropTable(
                name: "GroupContestConfig");

            migrationBuilder.DropTable(
                name: "GroupJoin");

            migrationBuilder.DropTable(
                name: "Judge");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "VotesRecord");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropTable(
                name: "MessageContent");

            migrationBuilder.DropTable(
                name: "Contest");

            migrationBuilder.DropTable(
                name: "Problem");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
