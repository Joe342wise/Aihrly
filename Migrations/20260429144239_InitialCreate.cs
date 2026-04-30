using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aihrly.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateName = table.Column<string>(type: "text", nullable: false),
                    CandidateEmail = table.Column<string>(type: "text", nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    Stage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CultureFitScore = table.Column<int>(type: "integer", nullable: true),
                    CultureFitComment = table.Column<string>(type: "text", nullable: true),
                    CultureFitUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CultureFitUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InterviewScore = table.Column<int>(type: "integer", nullable: true),
                    InterviewComment = table.Column<string>(type: "text", nullable: true),
                    InterviewUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InterviewUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessmentScore = table.Column<int>(type: "integer", nullable: true),
                    AssessmentComment = table.Column<string>(type: "text", nullable: true),
                    AssessmentUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_TeamMembers_AssessmentUpdatedBy",
                        column: x => x.AssessmentUpdatedBy,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Applications_TeamMembers_CultureFitUpdatedBy",
                        column: x => x.CultureFitUpdatedBy,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Applications_TeamMembers_InterviewUpdatedBy",
                        column: x => x.InterviewUpdatedBy,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationNotes_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationNotes_TeamMembers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationNotes_TeamMembers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StageHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStage = table.Column<string>(type: "text", nullable: false),
                    ToStage = table.Column<string>(type: "text", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageHistories_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageHistories_TeamMembers_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StageHistories_TeamMembers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "TeamMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "TeamMembers",
                columns: new[] { "Id", "Email", "Name", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "sarah@aihrly.com", "Sarah Chen", "recruiter" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "marcus@aihrly.com", "Marcus Johnson", "hiring_manager" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "priya@aihrly.com", "Priya Patel", "recruiter" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_ApplicationId",
                table: "ApplicationNotes",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_CreatedBy",
                table: "ApplicationNotes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_CreatedByUserId",
                table: "ApplicationNotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AssessmentUpdatedBy",
                table: "Applications",
                column: "AssessmentUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CultureFitUpdatedBy",
                table: "Applications",
                column: "CultureFitUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_InterviewUpdatedBy",
                table: "Applications",
                column: "InterviewUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_CandidateEmail",
                table: "Applications",
                columns: new[] { "JobId", "CandidateEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageHistories_ApplicationId",
                table: "StageHistories",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_StageHistories_ChangedBy",
                table: "StageHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StageHistories_ChangedByUserId",
                table: "StageHistories",
                column: "ChangedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationNotes");

            migrationBuilder.DropTable(
                name: "StageHistories");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "TeamMembers");
        }
    }
}
