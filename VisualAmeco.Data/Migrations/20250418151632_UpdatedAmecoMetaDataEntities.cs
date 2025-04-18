using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisualAmeco.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAmecoMetaDataEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Indicators_Subchapters_SubChapterId",
                table: "Indicators");

            migrationBuilder.DropForeignKey(
                name: "FK_Values_Indicators_VariableId",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Values_VariableId",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Subchapters_ChapterId",
                table: "Subchapters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Indicators",
                table: "Indicators");

            migrationBuilder.RenameTable(
                name: "Indicators",
                newName: "Variables");

            migrationBuilder.RenameIndex(
                name: "IX_Indicators_SubChapterId",
                table: "Variables",
                newName: "IX_Variables_SubChapterId");

            migrationBuilder.RenameIndex(
                name: "IX_Indicators_Code",
                table: "Variables",
                newName: "IX_Variables_Code");

            migrationBuilder.AddColumn<int>(
                name: "Agg",
                table: "Variables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ref",
                table: "Variables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Trn",
                table: "Variables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitCode",
                table: "Variables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Variables",
                table: "Variables",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Values_VariableId_CountryId_Year_Month",
                table: "Values",
                columns: new[] { "VariableId", "CountryId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subchapters_ChapterId_Name",
                table: "Subchapters",
                columns: new[] { "ChapterId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Values_Variables_VariableId",
                table: "Values",
                column: "VariableId",
                principalTable: "Variables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Variables_Subchapters_SubChapterId",
                table: "Variables",
                column: "SubChapterId",
                principalTable: "Subchapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Values_Variables_VariableId",
                table: "Values");

            migrationBuilder.DropForeignKey(
                name: "FK_Variables_Subchapters_SubChapterId",
                table: "Variables");

            migrationBuilder.DropIndex(
                name: "IX_Values_VariableId_CountryId_Year_Month",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Subchapters_ChapterId_Name",
                table: "Subchapters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Variables",
                table: "Variables");

            migrationBuilder.DropColumn(
                name: "Agg",
                table: "Variables");

            migrationBuilder.DropColumn(
                name: "Ref",
                table: "Variables");

            migrationBuilder.DropColumn(
                name: "Trn",
                table: "Variables");

            migrationBuilder.DropColumn(
                name: "UnitCode",
                table: "Variables");

            migrationBuilder.RenameTable(
                name: "Variables",
                newName: "Indicators");

            migrationBuilder.RenameIndex(
                name: "IX_Variables_SubChapterId",
                table: "Indicators",
                newName: "IX_Indicators_SubChapterId");

            migrationBuilder.RenameIndex(
                name: "IX_Variables_Code",
                table: "Indicators",
                newName: "IX_Indicators_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Indicators",
                table: "Indicators",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Values_VariableId",
                table: "Values",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_Subchapters_ChapterId",
                table: "Subchapters",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Indicators_Subchapters_SubChapterId",
                table: "Indicators",
                column: "SubChapterId",
                principalTable: "Subchapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Values_Indicators_VariableId",
                table: "Values",
                column: "VariableId",
                principalTable: "Indicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
