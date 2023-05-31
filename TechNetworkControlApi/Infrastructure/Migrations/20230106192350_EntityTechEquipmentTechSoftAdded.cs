using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNetworkControlApi.Infrastructure.Migrations
{
    public partial class EntityTechEquipmentTechSoftAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechEquipmentTechSoft_TechEquipments_TechEquipmentId",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropForeignKey(
                name: "FK_TechEquipmentTechSoft_TechSofts_SoftId",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TechEquipmentTechSoft",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropIndex(
                name: "IX_TechEquipmentTechSoft_TechEquipmentId",
                table: "TechEquipmentTechSoft");

            migrationBuilder.RenameColumn(
                name: "TechEquipmentId",
                table: "TechEquipmentTechSoft",
                newName: "TechEquipmentId");

            migrationBuilder.RenameColumn(
                name: "SoftId",
                table: "TechEquipmentTechSoft",
                newName: "TechSoftId");

            migrationBuilder.AddColumn<DateTime>(
                name: "InstalledDate",
                table: "TechEquipmentTechSoft",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TechEquipmentTechSoft",
                table: "TechEquipmentTechSoft",
                columns: new[] { "TechEquipmentId", "TechSoftId" });

            migrationBuilder.CreateIndex(
                name: "IX_TechEquipmentTechSoft_TechSoftId",
                table: "TechEquipmentTechSoft",
                column: "TechSoftId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechEquipmentTechSoft_TechEquipments_TechEquipmentId",
                table: "TechEquipmentTechSoft",
                column: "TechEquipmentId",
                principalTable: "TechEquipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TechEquipmentTechSoft_TechSofts_TechSoftId",
                table: "TechEquipmentTechSoft",
                column: "TechSoftId",
                principalTable: "TechSofts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechEquipmentTechSoft_TechEquipments_TechEquipmentId",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropForeignKey(
                name: "FK_TechEquipmentTechSoft_TechSofts_TechSoftId",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TechEquipmentTechSoft",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropIndex(
                name: "IX_TechEquipmentTechSoft_TechSoftId",
                table: "TechEquipmentTechSoft");

            migrationBuilder.DropColumn(
                name: "InstalledDate",
                table: "TechEquipmentTechSoft");

            migrationBuilder.RenameColumn(
                name: "TechSoftId",
                table: "TechEquipmentTechSoft",
                newName: "SoftId")
                .Annotation("Relational:ColumnType", "int(11)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TechEquipmentTechSoft",
                table: "TechEquipmentTechSoft",
                columns: new[] { "SoftId", "TechEquipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TechEquipmentTechSoft_TechEquipmentId",
                table: "TechEquipmentTechSoft",
                column: "TechEquipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechEquipmentTechSoft_TechEquipments_TechEquipmentId",
                table: "TechEquipmentTechSoft",
                column: "TechEquipmentId",
                principalTable: "TechEquipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TechEquipmentTechSoft_TechSofts_SoftId",
                table: "TechEquipmentTechSoft",
                column: "SoftId",
                principalTable: "TechSofts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
