using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProtocolSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSyntaxHighlightingEnabled",
                table: "SettingsProtocol",
                newName: "UseSyntaxHighlighting");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UseSyntaxHighlighting",
                table: "SettingsProtocol",
                newName: "IsSyntaxHighlightingEnabled");
        }
    }
}
