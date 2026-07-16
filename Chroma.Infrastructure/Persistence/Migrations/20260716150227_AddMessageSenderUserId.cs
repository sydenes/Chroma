using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageSenderUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderDisplayName",
                table: "messages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderUserId",
                table: "messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_SenderUserId",
                table: "messages",
                column: "SenderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_messages_users_SenderUserId",
                table: "messages",
                column: "SenderUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_messages_users_SenderUserId",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "IX_messages_SenderUserId",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "SenderDisplayName",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                table: "messages");
        }
    }
}
