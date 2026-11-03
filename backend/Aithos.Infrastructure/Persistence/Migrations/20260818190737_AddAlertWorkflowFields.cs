using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aithos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedAt",
                table: "Alerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Alerts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcknowledgedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Alerts");
        }
    }
}
