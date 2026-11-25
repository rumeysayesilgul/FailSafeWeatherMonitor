using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Failsafe_Weather_Monitor.Migrations;

/// <inheritdoc />
public partial class _20261125135627_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WeatherDataPoints",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                Temperature = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                Humidity = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                DataSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WeatherDataPoints", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WeatherDataPoints_StationName_Timestamp",
            table: "WeatherDataPoints",
            columns: new[] { "StationName", "Timestamp" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WeatherDataPoints");
    }
}
