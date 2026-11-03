using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aithos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyOperationalDataset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroundTruthEvents");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Readings_TowerId_RecordedAt",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "GridConsumption",
                table: "Readings");

            migrationBuilder.AlterColumn<decimal>(
                name: "WindSpeed",
                table: "Readings",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldPrecision: 6,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PowerOutput",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.Sql(
                """
                WITH DuplicateReadings AS
                (
                    SELECT Id,
                           ROW_NUMBER() OVER (
                               PARTITION BY TowerId, RecordedAt
                               ORDER BY Id
                           ) AS DuplicateNumber
                    FROM Readings
                )
                DELETE FROM DuplicateReadings WHERE DuplicateNumber > 1;
                """
            );

            migrationBuilder.CreateIndex(
                name: "IX_Readings_TowerId_RecordedAt",
                table: "Readings",
                columns: new[] { "TowerId", "RecordedAt" },
                unique: true);

            migrationBuilder.Sql(
                """
                UPDATE TowerAnalyses
                SET Engine = 'Telecommunications Project Core AI'
                WHERE Engine = 'Aithos Core AI';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE TowerAnalyses
                SET Engine = 'Aithos Core AI'
                WHERE Engine = 'Telecommunications Project Core AI';
                """
            );

            migrationBuilder.DropIndex(
                name: "IX_Readings_TowerId_RecordedAt",
                table: "Readings");

            migrationBuilder.AlterColumn<decimal>(
                name: "WindSpeed",
                table: "Readings",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PowerOutput",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GridConsumption",
                table: "Readings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "GroundTruthEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TowerId = table.Column<int>(type: "int", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExpectedDetection = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Magnitude = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    SeverityClass = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

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
        }
    }
}
