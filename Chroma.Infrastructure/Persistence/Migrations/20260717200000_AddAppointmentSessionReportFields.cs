using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSessionReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NextSteps",
                table: "appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateNotes",
                table: "appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressScore",
                table: "appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionSummary",
                table: "appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionType",
                table: "appointments",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "follow_up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextSteps",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "PrivateNotes",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "ProgressScore",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "SessionSummary",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "SessionType",
                table: "appointments");
        }
    }
}
