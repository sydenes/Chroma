using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chroma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FilterContactChannelUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contact_channels_TenantId_ChannelType_Value",
                table: "contact_channels");

            migrationBuilder.CreateIndex(
                name: "IX_contact_channels_TenantId_ChannelType_Value",
                table: "contact_channels",
                columns: new[] { "TenantId", "ChannelType", "Value" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contact_channels_TenantId_ChannelType_Value",
                table: "contact_channels");

            migrationBuilder.CreateIndex(
                name: "IX_contact_channels_TenantId_ChannelType_Value",
                table: "contact_channels",
                columns: new[] { "TenantId", "ChannelType", "Value" },
                unique: true);
        }
    }
}
