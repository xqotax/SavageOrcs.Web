using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavageOrcs.DbContext.Migrations
{
    public partial class GoodChangesisAreas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "Lvl_1",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "Lvl_2",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "Lvl_3",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "Lvl_4",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "AreaTypeId",
                table: "Areas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}
