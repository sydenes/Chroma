using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskBoardModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "task_boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "task_columns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_columns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_columns_task_boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "task_boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_labels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Color = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_labels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_labels_task_boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "task_boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssigneeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_cards_contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_task_cards_task_boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "task_boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_cards_task_columns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "task_columns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_task_cards_users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_task_cards_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "task_card_labels",
                columns: table => new
                {
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_card_labels", x => new { x.CardId, x.LabelId });
                    table.ForeignKey(
                        name: "FK_task_card_labels_task_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "task_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_card_labels_task_labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "task_labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_comments_task_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "task_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_comments_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_boards_TenantId_IsDefault",
                table: "task_boards",
                columns: new[] { "TenantId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_task_card_labels_LabelId",
                table: "task_card_labels",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_AssigneeUserId",
                table: "task_cards",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_BoardId",
                table: "task_cards",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_ColumnId_SortOrder",
                table: "task_cards",
                columns: new[] { "ColumnId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_ContactId",
                table: "task_cards",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_CreatedByUserId",
                table: "task_cards",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_TenantId_AssigneeUserId",
                table: "task_cards",
                columns: new[] { "TenantId", "AssigneeUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_task_cards_TenantId_BoardId",
                table: "task_cards",
                columns: new[] { "TenantId", "BoardId" });

            migrationBuilder.CreateIndex(
                name: "IX_task_columns_BoardId_SortOrder",
                table: "task_columns",
                columns: new[] { "BoardId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_task_columns_TenantId",
                table: "task_columns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_task_comments_AuthorUserId",
                table: "task_comments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_task_comments_CardId_CreatedAtUtc",
                table: "task_comments",
                columns: new[] { "CardId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_task_labels_BoardId_Name",
                table: "task_labels",
                columns: new[] { "BoardId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_card_labels");

            migrationBuilder.DropTable(
                name: "task_comments");

            migrationBuilder.DropTable(
                name: "task_labels");

            migrationBuilder.DropTable(
                name: "task_cards");

            migrationBuilder.DropTable(
                name: "task_columns");

            migrationBuilder.DropTable(
                name: "task_boards");
        }
    }
}
