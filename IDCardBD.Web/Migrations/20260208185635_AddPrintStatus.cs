using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDCardBD.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrintStatus",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrintStatus",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintStatus",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PrintStatus",
                table: "Employees");
        }
    }
}
