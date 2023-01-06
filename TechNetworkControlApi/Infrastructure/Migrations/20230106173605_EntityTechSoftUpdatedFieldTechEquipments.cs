using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNetworkControlApi.Infrastructure.Migrations
{
    public partial class EntityTechSoftUpdatedFieldTechEquipments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechSofts_TechEquipments_TechEquipmentId",
                table: "TechSofts");

            migrationBuilder.DropIndex(
                name: "IX_TechSofts_TechEquipmentId",
                table: "TechSofts");

            migrationBuilder.DropColumn(
                name: "TechEquipmentId",
                table: "TechSofts");

            migrationBuilder.AlterColumn<int>(
                name: "UserToId",
                table: "RepairRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "TechEquipmentTechSoft",
                columns: table => new
                {
                    SoftId = table.Column<int>(type: "int", nullable: false),
                    TechEquipmentId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechEquipmentTechSoft", x => new {x.SoftId, x.TechEquipmentId });
                    table.ForeignKey(
                        name: "FK_TechEquipmentTechSoft_TechEquipments_TechEquipmentId",
                        column: x => x.TechEquipmentId,
                        principalTable: "TechEquipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechEquipmentTechSoft_TechSofts_SoftId",
                        column: x => x.SoftId,
                        principalTable: "TechSofts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TechEquipmentTechSoft_TechEquipmentId",
                table: "TechEquipmentTechSoft",
                column: "TechEquipmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechEquipmentTechSoft");

            migrationBuilder.AddColumn<string>(
                name: "TechEquipmentId",
                table: "TechSofts",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "UserToId",
                table: "RepairRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechSofts_TechEquipmentId",
                table: "TechSofts",
                column: "TechEquipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechSofts_TechEquipments_TechEquipmentId",
                table: "TechSofts",
                column: "TechEquipmentId",
                principalTable: "TechEquipments",
                principalColumn: "Id");
        }
    }
}
