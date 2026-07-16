using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactPotentialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "contacts",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "TRY");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedValue",
                table: "contacts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleStage",
                table: "contacts",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "new");

            migrationBuilder.AddColumn<string>(
                name: "PotentialType",
                table: "contacts",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "lead");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_TenantId_PotentialType_LifecycleStage",
                table: "contacts",
                columns: new[] { "TenantId", "PotentialType", "LifecycleStage" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contacts_TenantId_PotentialType_LifecycleStage",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "EstimatedValue",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "LifecycleStage",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "PotentialType",
                table: "contacts");
        }
    }
}
