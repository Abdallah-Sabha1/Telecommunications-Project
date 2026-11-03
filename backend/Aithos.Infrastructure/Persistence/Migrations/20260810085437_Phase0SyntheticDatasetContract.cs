using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aithos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase0SyntheticDatasetContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Readings_TowerId",
                table: "Readings");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Towers",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Towers",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseLoadKwh",
                table: "Towers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CoolingCoef",
                table: "Towers",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetBurnRateLph",
                table: "Towers",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Towers",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SiteId",
                table: "Towers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TankCapacityL",
                table: "Towers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TrafficCoef",
                table: "Towers",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cdd",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelLevelEnd",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelLevelStart",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GensetRunHours",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OutageHours",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RefillFlag",
                table: "Readings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TrafficPeakPct",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE Towers
                SET SiteId = CASE
                    WHEN ISNULL(SiteId, '') = '' THEN CONCAT('SITE-', RIGHT(CONCAT('000', CAST(Id AS varchar(3))), 3))
                    ELSE SiteId
                END,
                [Group] = CASE
                    WHEN ISNULL([Group], '') = '' THEN 'Legacy'
                    ELSE [Group]
                END,
                BaseLoadKwh = CASE
                    WHEN BaseLoadKwh = 0 THEN 30
                    ELSE BaseLoadKwh
                END,
                CoolingCoef = CASE
                    WHEN CoolingCoef = 0 THEN 0.10
                    ELSE CoolingCoef
                END,
                TrafficCoef = CASE
                    WHEN TrafficCoef = 0 THEN 0.35
                    ELSE TrafficCoef
                END,
                GensetBurnRateLph = CASE
                    WHEN GensetBurnRateLph = 0 THEN 2.50
                    ELSE GensetBurnRateLph
                END,
                TankCapacityL = CASE
                    WHEN TankCapacityL = 0 THEN 1200
                    ELSE TankCapacityL
                END;
            """);

            migrationBuilder.CreateTable(
                name: "GroundTruthEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TowerId = table.Column<int>(type: "int", nullable: false),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Magnitude = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    SeverityClass = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ExpectedDetection = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroundTruthEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroundTruthEvents_Towers_TowerId",
                        column: x => x.TowerId,
                        principalTable: "Towers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DieselPrice = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    ElectricityPrice = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyPrices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Towers_Group",
                table: "Towers",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Towers_Name",
                table: "Towers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Towers_SiteId",
                table: "Towers",
                column: "SiteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readings_RecordedAt",
                table: "Readings",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Readings_TowerId_RecordedAt",
                table: "Readings",
                columns: new[] { "TowerId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GroundTruthEvents_EventId",
                table: "GroundTruthEvents",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroundTruthEvents_TowerId_DateStart",
                table: "GroundTruthEvents",
                columns: new[] { "TowerId", "DateStart" });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPrices_Month",
                table: "MonthlyPrices",
                column: "Month",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroundTruthEvents");

            migrationBuilder.DropTable(
                name: "MonthlyPrices");

            migrationBuilder.DropIndex(
                name: "IX_Towers_Group",
                table: "Towers");

            migrationBuilder.DropIndex(
                name: "IX_Towers_Name",
                table: "Towers");

            migrationBuilder.DropIndex(
                name: "IX_Towers_SiteId",
                table: "Towers");

            migrationBuilder.DropIndex(
                name: "IX_Readings_RecordedAt",
                table: "Readings");

            migrationBuilder.DropIndex(
                name: "IX_Readings_TowerId_RecordedAt",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "BaseLoadKwh",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "CoolingCoef",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "GensetBurnRateLph",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "TankCapacityL",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "TrafficCoef",
                table: "Towers");

            migrationBuilder.DropColumn(
                name: "Cdd",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "FuelLevelEnd",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "FuelLevelStart",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "GensetRunHours",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "OutageHours",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "RefillFlag",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "TrafficPeakPct",
                table: "Readings");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Towers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Towers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.CreateIndex(
                name: "IX_Readings_TowerId",
                table: "Readings",
                column: "TowerId");
        }
    }
}
