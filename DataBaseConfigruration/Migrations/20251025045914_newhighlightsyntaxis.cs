using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
    /// <inheritdoc />
    public partial class newhighlightsyntaxis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseSyntaxHighlighting",
                table: "SettingsProtocol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseSyntaxHighlighting",
                table: "SettingsProtocol",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
