using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aithos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTowerAnalysisRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TowerAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TowerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TelemetryTimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyFindingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PossibleCausesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationalImpact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedFinancialLoss = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedCarbonImpact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedActionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TowerAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TowerAnalyses_Towers_TowerId",
                        column: x => x.TowerId,
                        principalTable: "Towers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TowerAnalyses_TowerId_CreatedAt",
                table: "TowerAnalyses",
                columns: new[] { "TowerId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TowerAnalyses");
        }
    }
}
