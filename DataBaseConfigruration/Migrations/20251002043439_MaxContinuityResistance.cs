using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
  /// <inheritdoc />
  public partial class MaxContinuityResistance : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "CleanTextErrorsProtocol",
          table: "SettingsProtocol",
          type: "TEXT",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<int>(
          name: "MaxContinuityResistance",
          table: "FastMeters",
          type: "INTEGER",
          nullable: false,
          defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "CleanTextErrorsProtocol",
          table: "SettingsProtocol");

      migrationBuilder.DropColumn(
          name: "MaxContinuityResistance",
          table: "FastMeters");
    }
  }
}
