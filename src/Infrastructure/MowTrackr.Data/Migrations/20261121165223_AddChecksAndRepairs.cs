using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MowTrackr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChecksAndRepairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "WorkOrderTasks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Choices",
                table: "WorkOrderTasks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "WorkOrderTasks",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "YesNo");

            migrationBuilder.AddColumn<string>(
                name: "Phase",
                table: "WorkOrderTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Service");

            migrationBuilder.AddColumn<string>(
                name: "RepairOn",
                table: "WorkOrderTasks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RepairWorkOrderId",
                table: "WorkOrderTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceTemplateId",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Service");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentWorkOrderId",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PostCheckSubmittedAt",
                table: "WorkOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PreCheckSubmittedAt",
                table: "WorkOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "ServiceTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Always");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetTypeId",
                table: "InspectionTemplates",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Timing",
                table: "InspectionTemplates",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "After");

            migrationBuilder.AddColumn<string>(
                name: "Choices",
                table: "InspectionTasks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "InspectionTasks",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "YesNo");

            migrationBuilder.AddColumn<string>(
                name: "RepairOn",
                table: "InspectionTasks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasTires",
                table: "AssetTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ServiceTemplateAssetTypes",
                columns: table => new
                {
                    AssetTypesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTemplateAssetTypes", x => new { x.AssetTypesId, x.ServiceTemplateId });
                    table.ForeignKey(
                        name: "FK_ServiceTemplateAssetTypes_AssetTypes_AssetTypesId",
                        column: x => x.AssetTypesId,
                        principalTable: "AssetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceTemplateAssetTypes_ServiceTemplates_ServiceTemplateId",
                        column: x => x.ServiceTemplateId,
                        principalTable: "ServiceTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTemplateInspections",
                columns: table => new
                {
                    InspectionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceTemplatesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTemplateInspections", x => new { x.InspectionsId, x.ServiceTemplatesId });
                    table.ForeignKey(
                        name: "FK_ServiceTemplateInspections_InspectionTemplates_InspectionsId",
                        column: x => x.InspectionsId,
                        principalTable: "InspectionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceTemplateInspections_ServiceTemplates_ServiceTemplatesId",
                        column: x => x.ServiceTemplatesId,
                        principalTable: "ServiceTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ParentWorkOrderId",
                table: "WorkOrders",
                column: "ParentWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTemplateAssetTypes_ServiceTemplateId",
                table: "ServiceTemplateAssetTypes",
                column: "ServiceTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTemplateInspections_ServiceTemplatesId",
                table: "ServiceTemplateInspections",
                column: "ServiceTemplatesId");

            // Each service template used to belong to exactly one asset type. Keep that: copy it into the new join table
            // before the old column goes, so existing templates keep applying to the same equipment.
            migrationBuilder.Sql(
                "INSERT INTO ServiceTemplateAssetTypes (AssetTypesId, ServiceTemplateId) SELECT AssetTypeId, Id FROM ServiceTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTemplates_AssetTypes_AssetTypeId",
                table: "ServiceTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTemplates_AssetTypeId",
                table: "ServiceTemplates");

            migrationBuilder.DropColumn(
                name: "AssetTypeId",
                table: "ServiceTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceTemplateAssetTypes");

            migrationBuilder.DropTable(
                name: "ServiceTemplateInspections");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ParentWorkOrderId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "Choices",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "Phase",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "RepairOn",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "RepairWorkOrderId",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ParentWorkOrderId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PostCheckSubmittedAt",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PreCheckSubmittedAt",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "Timing",
                table: "InspectionTemplates");

            migrationBuilder.DropColumn(
                name: "Choices",
                table: "InspectionTasks");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "InspectionTasks");

            migrationBuilder.DropColumn(
                name: "RepairOn",
                table: "InspectionTasks");

            migrationBuilder.DropColumn(
                name: "HasTires",
                table: "AssetTypes");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceTemplateId",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssetTypeId",
                table: "ServiceTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetTypeId",
                table: "InspectionTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTemplates_AssetTypeId",
                table: "ServiceTemplates",
                column: "AssetTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTemplates_AssetTypes_AssetTypeId",
                table: "ServiceTemplates",
                column: "AssetTypeId",
                principalTable: "AssetTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
