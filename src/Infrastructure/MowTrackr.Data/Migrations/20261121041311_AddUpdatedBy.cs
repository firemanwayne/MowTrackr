using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MowTrackr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "WorkOrderTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "UserInvitations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ServiceTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ServiceTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Mowers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Manufacturers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ManufacturerModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "InspectionTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "InspectionTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "AssetTypes",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "WorkOrderTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserInvitations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ServiceTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Mowers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ManufacturerModels");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InspectionTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InspectionTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AssetTypes");
        }
    }
}
