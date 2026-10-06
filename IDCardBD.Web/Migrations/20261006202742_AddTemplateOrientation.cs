using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDCardBD.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateOrientation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Orientation",
                table: "CardTemplates",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Orientation",
                table: "CardTemplates");
        }
    }
}
