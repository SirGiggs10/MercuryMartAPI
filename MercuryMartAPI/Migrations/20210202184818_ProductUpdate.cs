using Microsoft.EntityFrameworkCore.Migrations;

namespace MercuryMartAPI.Migrations
{
    public partial class ProductUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ProductCost",
                table: "Product",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCost",
                table: "Product");
        }
    }
}
