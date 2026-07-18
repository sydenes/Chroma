using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationLastMessagePreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastMessagePreview",
                table: "conversations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "conversations" AS c
                SET "LastMessagePreview" = (
                    SELECT LEFT(
                        REGEXP_REPLACE(
                            COALESCE(NULLIF(BTRIM(m."Text"), ''), f."FileName", ''),
                            '\s+',
                            ' ',
                            'g'),
                        500)
                    FROM "messages" AS m
                    LEFT JOIN "files" AS f ON f."Id" = m."FileId" AND f."IsDeleted" = false
                    WHERE m."ConversationId" = c."Id" AND m."IsDeleted" = false
                    ORDER BY m."SentAtUtc" DESC, m."Id" DESC
                    LIMIT 1
                )
                WHERE c."LastMessagePreview" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessagePreview",
                table: "conversations");
        }
    }
}
