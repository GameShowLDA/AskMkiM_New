using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
    /// <inheritdoc />
    public partial class archive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserArchiveRootEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FolderPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    SearchRecursively = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsEncryptionPlanned = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserArchiveRootEntities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserArchiveRootEntities_FolderPath",
                table: "UserArchiveRootEntities",
                column: "FolderPath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserArchiveRootEntities");
        }
    }
}
