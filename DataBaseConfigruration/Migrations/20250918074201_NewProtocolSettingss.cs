using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
    /// <inheritdoc />
    public partial class NewProtocolSettingss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettingsProtocol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShowDeviceInfo = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoSaveProtocol = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoPrintProtocol = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOperationTime = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowDetailedProtocol = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowProtocolInSoftware = table.Column<bool>(type: "INTEGER", nullable: false),
                    GenerateProtocol = table.Column<bool>(type: "INTEGER", nullable: false),
                    BaseTextProtocol = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorTextProtocol = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsProtocol", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettingsProtocol");
        }
    }
}
