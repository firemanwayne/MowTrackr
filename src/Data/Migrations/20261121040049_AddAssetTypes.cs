using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MowTrackr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypes_Name",
                table: "AssetTypes",
                column: "Name",
                unique: true);

            // Starter types. Existing mowers and service templates are put under "Riding Mower" and can be changed afterwards.
            migrationBuilder.Sql(
                """
                INSERT INTO AssetTypes (Id, Name, CreatedAt, UpdatedAt) VALUES
                    ('6f1c2a10-0000-4000-8000-000000000001', 'Riding Mower', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('6f1c2a10-0000-4000-8000-000000000002', 'Push Mower', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('6f1c2a10-0000-4000-8000-000000000003', 'Weed Eater', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('6f1c2a10-0000-4000-8000-000000000004', 'Blower', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                    ('6f1c2a10-0000-4000-8000-000000000005', 'Chain Saw', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                """);

            foreach (var table in new[] { "Mowers", "ServiceTemplates" })
            {
                migrationBuilder.AddColumn<Guid>(
                    name: "AssetTypeId",
                    table: table,
                    type: "uniqueidentifier",
                    nullable: true);

                migrationBuilder.Sql($"UPDATE {table} SET AssetTypeId = '6f1c2a10-0000-4000-8000-000000000001'");

                migrationBuilder.AlterColumn<Guid>(
                    name: "AssetTypeId",
                    table: table,
                    type: "uniqueidentifier",
                    nullable: false,
                    oldClrType: typeof(Guid),
                    oldType: "uniqueidentifier",
                    oldNullable: true);

                migrationBuilder.CreateIndex(
                    name: $"IX_{table}_AssetTypeId",
                    table: table,
                    column: "AssetTypeId");

                migrationBuilder.AddForeignKey(
                    name: $"FK_{table}_AssetTypes_AssetTypeId",
                    table: table,
                    column: "AssetTypeId",
                    principalTable: "AssetTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mowers_AssetTypes_AssetTypeId",
                table: "Mowers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTemplates_AssetTypes_AssetTypeId",
                table: "ServiceTemplates");

            migrationBuilder.DropTable(
                name: "AssetTypes");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTemplates_AssetTypeId",
                table: "ServiceTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Mowers_AssetTypeId",
                table: "Mowers");

            migrationBuilder.DropColumn(
                name: "AssetTypeId",
                table: "ServiceTemplates");

            migrationBuilder.DropColumn(
                name: "AssetTypeId",
                table: "Mowers");
        }
    }
}
