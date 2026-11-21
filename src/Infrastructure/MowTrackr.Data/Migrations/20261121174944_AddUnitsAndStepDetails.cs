using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MowTrackr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsAndStepDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The oil quantity used to be quarts only. It keeps its values; each mower's unit is set to Quart below.
            migrationBuilder.RenameColumn(
                name: "OilQuantityQuarts",
                table: "Mowers",
                newName: "OilQuantity");

            migrationBuilder.AlterColumn<decimal>(
                name: "OilQuantity",
                table: "Mowers",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldPrecision: 6,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "NotesRequirement",
                table: "WorkOrderTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "PartNumber",
                table: "WorkOrderTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartNumberRequirement",
                table: "WorkOrderTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "WorkOrderTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "WorkOrderTasks",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuantityRequirement",
                table: "WorkOrderTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "QuantityUnit",
                table: "WorkOrderTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StepNotes",
                table: "WorkOrderTasks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotesRequirement",
                table: "ServiceTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "PartNumberRequirement",
                table: "ServiceTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "QuantityRequirement",
                table: "ServiceTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "QuantityUnitSource",
                table: "ServiceTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "StepUnit");

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "ServiceTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OilTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_UnitId",
                table: "ServiceTasks",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OilTypes_Name",
                table: "OilTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_Abbreviation",
                table: "UnitsOfMeasure",
                column: "Abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_Name",
                table: "UnitsOfMeasure",
                column: "Name",
                unique: true);

            // Starter units. Add more under Admin > Units.
            migrationBuilder.Sql(
                """
                INSERT INTO UnitsOfMeasure (Id, Name, Abbreviation, CreatedAt, UpdatedAt) VALUES
                    ('7a1c2a10-0000-4000-8000-000000000001', 'Quart', 'qt', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('7a1c2a10-0000-4000-8000-000000000002', 'Ounce', 'oz', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('7a1c2a10-0000-4000-8000-000000000003', 'Gallon', 'gal', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('7a1c2a10-0000-4000-8000-000000000004', 'Pint', 'pt', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('7a1c2a10-0000-4000-8000-000000000005', 'Each', 'ea', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('7a1c2a10-0000-4000-8000-000000000006', 'Liter', 'L', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('7a1c2a10-0000-4000-8000-000000000007', 'Milliliter', 'mL', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "OilUnitId",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("UPDATE Mowers SET OilUnitId = '7a1c2a10-0000-4000-8000-000000000001'");

            migrationBuilder.AlterColumn<Guid>(
                name: "OilUnitId",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mowers_OilUnitId",
                table: "Mowers",
                column: "OilUnitId");

            // Each mower's free-text oil type becomes an entry in the new oil type list, and the mower points at it. A few common
            // oils are added as well so there is something to pick from on a fresh database.
            migrationBuilder.AddColumn<Guid>(
                name: "OilTypeId",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO OilTypes (Id, Name, CreatedAt, UpdatedAt)
                SELECT NEWID(), v.Name, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
                FROM (SELECT DISTINCT LTRIM(RTRIM(OilType)) AS Name FROM Mowers WHERE LTRIM(RTRIM(OilType)) <> '') v;

                INSERT INTO OilTypes (Id, Name, CreatedAt, UpdatedAt)
                SELECT NEWID(), s.Name, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
                FROM (VALUES ('SAE 30'), ('SAE 10W-30'), ('SAE 5W-30'), ('5W-30 Synthetic'), ('2-Cycle Mix 50:1')) s(Name)
                WHERE NOT EXISTS (SELECT 1 FROM OilTypes o WHERE o.Name = s.Name);
                """);

            migrationBuilder.Sql("UPDATE m SET OilTypeId = o.Id FROM Mowers m JOIN OilTypes o ON o.Name = LTRIM(RTRIM(m.OilType))");

            migrationBuilder.Sql("UPDATE Mowers SET OilTypeId = (SELECT TOP 1 Id FROM OilTypes ORDER BY Name) WHERE OilTypeId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "OilTypeId",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "OilType",
                table: "Mowers");

            migrationBuilder.CreateIndex(
                name: "IX_Mowers_OilTypeId",
                table: "Mowers",
                column: "OilTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mowers_OilTypes_OilTypeId",
                table: "Mowers",
                column: "OilTypeId",
                principalTable: "OilTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mowers_UnitsOfMeasure_OilUnitId",
                table: "Mowers",
                column: "OilUnitId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTasks_UnitsOfMeasure_UnitId",
                table: "ServiceTasks",
                column: "UnitId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mowers_OilTypes_OilTypeId",
                table: "Mowers");

            migrationBuilder.DropForeignKey(
                name: "FK_Mowers_UnitsOfMeasure_OilUnitId",
                table: "Mowers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTasks_UnitsOfMeasure_UnitId",
                table: "ServiceTasks");

            migrationBuilder.DropTable(
                name: "OilTypes");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTasks_UnitId",
                table: "ServiceTasks");

            migrationBuilder.DropIndex(
                name: "IX_Mowers_OilTypeId",
                table: "Mowers");

            migrationBuilder.DropIndex(
                name: "IX_Mowers_OilUnitId",
                table: "Mowers");

            migrationBuilder.DropColumn(
                name: "NotesRequirement",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "PartNumberRequirement",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "Product",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "QuantityRequirement",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "QuantityUnit",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "StepNotes",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "NotesRequirement",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "PartNumberRequirement",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "QuantityRequirement",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "QuantityUnitSource",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "ServiceTasks");

            migrationBuilder.RenameColumn(
                name: "OilQuantity",
                table: "Mowers",
                newName: "OilQuantityQuarts");

            migrationBuilder.AlterColumn<decimal>(
                name: "OilQuantityQuarts",
                table: "Mowers",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldPrecision: 8,
                oldScale: 2);

            migrationBuilder.DropColumn(
                name: "OilTypeId",
                table: "Mowers");

            migrationBuilder.DropColumn(
                name: "OilUnitId",
                table: "Mowers");

            migrationBuilder.AddColumn<string>(
                name: "OilType",
                table: "Mowers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
