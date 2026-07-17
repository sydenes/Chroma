using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUploadMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "files",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "document");

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "files",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UploadedByUserId",
                table: "files",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE files
                SET "StorageKey" = TRIM(BOTH '/' FROM "Url")
                WHERE "StorageKey" = '' AND "Url" IS NOT NULL AND "Url" <> '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_files_TenantId_Category",
                table: "files",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_files_UploadedByUserId",
                table: "files",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_files_users_UploadedByUserId",
                table: "files",
                column: "UploadedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_users_UploadedByUserId",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_TenantId_Category",
                table: "files");

            migrationBuilder.DropIndex(
                name: "IX_files_UploadedByUserId",
                table: "files");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "files");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "files");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "files");
        }
    }
}
