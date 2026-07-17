using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSourceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "notifications",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_SourceType_SourceId",
                table: "notifications",
                columns: new[] { "UserId", "SourceType", "SourceId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"SourceType\" IS NOT NULL AND \"SourceId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_UserId_SourceType_SourceId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "notifications");
        }
    }
}
