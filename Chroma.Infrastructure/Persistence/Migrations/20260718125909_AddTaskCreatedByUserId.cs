using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_CreatedByUserId",
                table: "tasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_TenantId_CreatedByUserId",
                table: "tasks",
                columns: new[] { "TenantId", "CreatedByUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_users_CreatedByUserId",
                table: "tasks",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_users_CreatedByUserId",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_CreatedByUserId",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_TenantId_CreatedByUserId",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "tasks");
        }
    }
}
