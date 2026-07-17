using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupChatAndMessageFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGroup",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "conversations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_FileId",
                table: "messages",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_messages_files_FileId",
                table: "messages",
                column: "FileId",
                principalTable: "files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_messages_files_FileId",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "IX_messages_FileId",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "IsGroup",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "conversations");
        }
    }
}
