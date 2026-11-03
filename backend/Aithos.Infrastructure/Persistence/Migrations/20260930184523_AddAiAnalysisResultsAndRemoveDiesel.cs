using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aithos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysisResultsAndRemoveDiesel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<string>(maxLength: 50, nullable: false),
                    ObservationDate = table.Column<DateTime>(nullable: false),
                    ActualKwh = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    ExpectedKwh = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Residual = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    ScoreZ = table.Column<decimal>(type: "decimal(12,6)", nullable: false),
                    Classification = table.Column<string>(nullable: false),
                    Slope = table.Column<decimal>(type: "decimal(12,6)", nullable: true),
                    Reason = table.Column<string>(nullable: false),
                    WastedKwh = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    ElectricityPricePerKwh = table.Column<decimal>(type: "decimal(12,6)", nullable: false),
                    WastedCostJod = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    CarbonImpactKgCo2e = table.Column<decimal>(type: "decimal(12,4)", nullable: true),
                    CarbonEvaluated = table.Column<bool>(nullable: false),
                    ValidationStatus = table.Column<string>(nullable: false),
                    ModelVersion = table.Column<string>(nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AiAnalysisResults", x => x.Id));
            migrationBuilder.CreateIndex("IX_AiAnalysisResults_SiteId_ObservationDate", "AiAnalysisResults", new[] { "SiteId", "ObservationDate" }, unique: true);
            migrationBuilder.DropColumn("GensetBurnRateLph", "Towers");
            migrationBuilder.DropColumn("TankCapacityL", "Towers");
            migrationBuilder.DropColumn("DieselConsumption", "Readings");
            migrationBuilder.DropColumn("FuelLevelEnd", "Readings");
            migrationBuilder.DropColumn("FuelLevelStart", "Readings");
            migrationBuilder.DropColumn("GensetRunHours", "Readings");
            migrationBuilder.DropColumn("RefillFlag", "Readings");
            migrationBuilder.DropColumn("DieselPrice", "MonthlyPrices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
