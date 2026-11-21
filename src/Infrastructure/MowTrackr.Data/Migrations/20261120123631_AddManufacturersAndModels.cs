using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MowTrackr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturersAndModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturerModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturerModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturerModels_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerModels_ManufacturerId_Name",
                table: "ManufacturerModels",
                columns: new[] { "ManufacturerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_Name",
                table: "Manufacturers",
                column: "Name",
                unique: true);

            // Add the new key columns while the old text columns still exist, so existing mowers can be backfilled.
            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturerId",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturerModelId",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(@"INSERT INTO Manufacturers (Id, Name, CreatedAt, UpdatedAt)
SELECT NEWID(), x.Name, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM (SELECT MIN(Manufacturer) AS Name FROM Mowers GROUP BY Manufacturer) x");

            migrationBuilder.Sql(@"INSERT INTO ManufacturerModels (Id, ManufacturerId, Name, CreatedAt, UpdatedAt)
SELECT NEWID(), mf.Id, x.Name, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM (SELECT Manufacturer, MIN(Model) AS Name FROM Mowers GROUP BY Manufacturer, Model) x
JOIN Manufacturers mf ON mf.Name = x.Manufacturer");

            migrationBuilder.Sql(@"UPDATE m SET ManufacturerId = mf.Id, ManufacturerModelId = md.Id
FROM Mowers m
JOIN Manufacturers mf ON mf.Name = m.Manufacturer
JOIN ManufacturerModels md ON md.ManufacturerId = mf.Id AND md.Name = m.Model");

            // Starter manufacturers so the dropdown is usable before anyone adds their own.
            migrationBuilder.Sql(@"INSERT INTO Manufacturers (Id, Name, CreatedAt, UpdatedAt)
SELECT NEWID(), s.Name, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM (VALUES (N'Ariens'), (N'Bad Boy'), (N'Cub Cadet'), (N'Exmark'), (N'Ferris'), (N'Gravely'), (N'Husqvarna'), (N'Hustler'), (N'John Deere'), (N'Kubota'), (N'Scag'), (N'Toro')) AS s(Name)
WHERE NOT EXISTS (SELECT 1 FROM Manufacturers m WHERE m.Name = s.Name)");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Mowers");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Mowers");

            migrationBuilder.CreateIndex(
                name: "IX_Mowers_ManufacturerId",
                table: "Mowers",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Mowers_ManufacturerModelId",
                table: "Mowers",
                column: "ManufacturerModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mowers_ManufacturerModels_ManufacturerModelId",
                table: "Mowers",
                column: "ManufacturerModelId",
                principalTable: "ManufacturerModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mowers_Manufacturers_ManufacturerId",
                table: "Mowers",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mowers_ManufacturerModels_ManufacturerModelId",
                table: "Mowers");

            migrationBuilder.DropForeignKey(
                name: "FK_Mowers_Manufacturers_ManufacturerId",
                table: "Mowers");

            migrationBuilder.DropIndex(
                name: "IX_Mowers_ManufacturerId",
                table: "Mowers");

            migrationBuilder.DropIndex(
                name: "IX_Mowers_ManufacturerModelId",
                table: "Mowers");

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Mowers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Mowers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE m SET Manufacturer = mf.Name, Model = md.Name
FROM Mowers m
JOIN Manufacturers mf ON mf.Id = m.ManufacturerId
JOIN ManufacturerModels md ON md.Id = m.ManufacturerModelId");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "Mowers");

            migrationBuilder.DropColumn(
                name: "ManufacturerModelId",
                table: "Mowers");

            migrationBuilder.DropTable(
                name: "ManufacturerModels");

            migrationBuilder.DropTable(
                name: "Manufacturers");
        }
    }
}
