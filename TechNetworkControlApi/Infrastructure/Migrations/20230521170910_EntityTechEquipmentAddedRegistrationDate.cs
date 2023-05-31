using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNetworkControlApi.Infrastructure.Migrations
{
    public partial class EntityTechEquipmentAddedRegistrationDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                 name: "RefreshToken",
                 table: "Users",
                 type: "char(36)",
                 maxLength: 36,
                 nullable: true,
                 oldClrType: typeof(Guid),
                 oldType: "char(36)", 
                 oldMaxLength: 36)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("ALTER TABLE `Users` MODIFY COLUMN `RefreshTokenExpirationDate` datetime(6) NULL");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "TechEquipments",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "TechEquipments");
        }
    }
}
