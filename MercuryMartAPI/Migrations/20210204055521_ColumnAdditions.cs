using Microsoft.EntityFrameworkCore.Migrations;

namespace MercuryMartAPI.Migrations
{
    public partial class ColumnAdditions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "CustomerOrderGroup");

            migrationBuilder.AddColumn<int>(
                name: "AssignedTo",
                table: "Product",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "CustomerOrderGroup",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrderName",
                table: "CustomerOrder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CustomerOrderGroup");

            migrationBuilder.DropColumn(
                name: "OrderName",
                table: "CustomerOrder");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "CustomerOrderGroup",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
