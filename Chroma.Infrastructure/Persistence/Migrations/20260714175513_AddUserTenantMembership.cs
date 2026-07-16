using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTenantMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_TenantId_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserId_ExpiresAtUtc",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "refresh_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "user_tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_tenants_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_tenants_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                UPDATE refresh_tokens rt
                SET "TenantId" = u."TenantId"
                FROM users u
                WHERE rt."UserId" = u."Id";
                """);

            migrationBuilder.Sql("""
                INSERT INTO user_tenants (
                    "Id",
                    "UserId",
                    "TenantId",
                    "Status",
                    "IsDefault",
                    "CreatedAtUtc",
                    "UpdatedAtUtc",
                    "DeletedAtUtc",
                    "IsDeleted"
                )
                SELECT
                    u."Id",
                    u."Id",
                    u."TenantId",
                    'active',
                    true,
                    now(),
                    NULL,
                    NULL,
                    false
                FROM users u
                WHERE u."IsDeleted" = false
                  AND NOT EXISTS (
                      SELECT 1
                      FROM user_tenants ut
                      WHERE ut."UserId" = u."Id"
                        AND ut."TenantId" = u."TenantId"
                        AND ut."IsDeleted" = false
                  );
                """);

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId_TenantId_ExpiresAtUtc",
                table: "refresh_tokens",
                columns: new[] { "UserId", "TenantId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_TenantId_Status",
                table: "user_tenants",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_UserId_TenantId",
                table: "user_tenants",
                columns: new[] { "UserId", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserId_TenantId_ExpiresAtUtc",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE users u
                SET "TenantId" = ut."TenantId"
                FROM user_tenants ut
                WHERE ut."UserId" = u."Id"
                  AND ut."IsDeleted" = false;
                """);

            migrationBuilder.DropTable(
                name: "user_tenants");

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId_Email",
                table: "users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId_ExpiresAtUtc",
                table: "refresh_tokens",
                columns: new[] { "UserId", "ExpiresAtUtc" });
        }
    }
}
