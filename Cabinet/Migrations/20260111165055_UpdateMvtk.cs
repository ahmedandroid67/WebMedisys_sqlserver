using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cabinet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMvtk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployerId",
                table: "stock_movements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_EmployerId",
                table: "stock_movements",
                column: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movements_Employer_EmployerId",
                table: "stock_movements",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "IdEmployer",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_movements_Employer_EmployerId",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_EmployerId",
                table: "stock_movements");

            migrationBuilder.DropColumn(
                name: "EmployerId",
                table: "stock_movements");
        }
    }
}
