using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aihrly.Migrations
{
    /// <inheritdoc />
    public partial class FixDuplicateFkColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationNotes_TeamMembers_CreatedByUserId",
                table: "ApplicationNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_StageHistories_TeamMembers_ChangedByUserId",
                table: "StageHistories");

            migrationBuilder.DropIndex(
                name: "IX_StageHistories_ChangedByUserId",
                table: "StageHistories");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationNotes_CreatedByUserId",
                table: "ApplicationNotes");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId",
                table: "StageHistories");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ApplicationNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChangedByUserId",
                table: "StageHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ApplicationNotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageHistories_ChangedByUserId",
                table: "StageHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_CreatedByUserId",
                table: "ApplicationNotes",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationNotes_TeamMembers_CreatedByUserId",
                table: "ApplicationNotes",
                column: "CreatedByUserId",
                principalTable: "TeamMembers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StageHistories_TeamMembers_ChangedByUserId",
                table: "StageHistories",
                column: "ChangedByUserId",
                principalTable: "TeamMembers",
                principalColumn: "Id");
        }
    }
}
