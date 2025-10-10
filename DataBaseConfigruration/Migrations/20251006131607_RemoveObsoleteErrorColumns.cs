using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseConfiguration.Migrations
{
  /// <inheritdoc />
  public partial class RemoveObsoleteErrorColumns : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "NumericError",
          table: "MeasurementErrors");

      migrationBuilder.DropColumn(
          name: "PercentageError",
          table: "MeasurementErrors");

      migrationBuilder.CreateTable(
          name: "MeasurementErrorRanges",
          columns: table => new
          {
            Id = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            MinValue = table.Column<double>(type: "REAL", nullable: false),
            MaxValue = table.Column<double>(type: "REAL", nullable: true),
            NumericError = table.Column<double>(type: "REAL", nullable: false),
            PercentageError = table.Column<double>(type: "REAL", nullable: false),
            MeasurementErrorEntityId = table.Column<int>(type: "INTEGER", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_MeasurementErrorRanges", x => x.Id);
            table.ForeignKey(
                      name: "FK_MeasurementErrorRanges_MeasurementErrors_MeasurementErrorEntityId",
                      column: x => x.MeasurementErrorEntityId,
                      principalTable: "MeasurementErrors",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_MeasurementErrorRanges_MeasurementErrorEntityId_MinValue_MaxValue",
          table: "MeasurementErrorRanges",
          columns: new[] { "MeasurementErrorEntityId", "MinValue", "MaxValue" },
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "MeasurementErrorRanges");

      migrationBuilder.AddColumn<double>(
          name: "NumericError",
          table: "MeasurementErrors",
          type: "REAL",
          nullable: false,
          defaultValue: 0.0);

      migrationBuilder.AddColumn<double>(
          name: "PercentageError",
          table: "MeasurementErrors",
          type: "REAL",
          nullable: false,
          defaultValue: 0.0);
    }
  }
}
