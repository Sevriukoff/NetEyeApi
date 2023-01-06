using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNetworkControlApi.Infrastructure.Migrations
{
    public partial class EntityRepairRequestUpdatedFieldUserTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `RepairRequests` CHANGE `UserToId` `UserToId` INT(11) NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `RepairRequests` CHANGE `UserToId` `UserToId` INT(11) NOT NULL;");
        }
    }
}
