using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cabinet.Migrations
{
    public partial class AddStockAndCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the New Category Table
            migrationBuilder.CreateTable(
                name: "category_stock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_stock", x => x.Id);
                });

            // 2. Create the New Stock Table (Linking to Category)
            migrationBuilder.CreateTable(
                name: "stock",
                columns: table => new
                {
                    id_produit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom_produit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    obs = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantite = table.Column<int>(type: "int", nullable: false),
                    alarme = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock", x => x.id_produit);
                    table.ForeignKey(
                        name: "FK_stock_category_stock_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "category_stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 3. Create the Index for the Foreign Key
            migrationBuilder.CreateIndex(
                name: "IX_stock_CategoryId",
                table: "stock",
                column: "CategoryId");

            /* IMPORTANT: All other Table creations (Employer, Patient, etc.) 
               are removed from this block because they already exist in 
               your SQL database.
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "stock");
            migrationBuilder.DropTable(name: "category_stock");
        }
    }
}