using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
  /// <inheritdoc />
  public partial class newsettingsprotocol : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "BaseTextProtocol",
          table: "SettingsProtocol",
          newName: "CleanTextProtocol");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "CleanTextProtocol",
          table: "SettingsProtocol",
          newName: "BaseTextProtocol");
    }
  }
}
