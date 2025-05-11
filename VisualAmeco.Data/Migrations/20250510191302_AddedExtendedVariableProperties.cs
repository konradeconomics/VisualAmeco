using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisualAmeco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedExtendedVariableProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Variables");

            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "Variables",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "UnitDescription",
                table: "Variables",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitDescription",
                table: "Variables");

            migrationBuilder.AlterColumn<int>(
                name: "UnitCode",
                table: "Variables",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Variables",
                type: "text",
                nullable: true);
        }
    }
}
