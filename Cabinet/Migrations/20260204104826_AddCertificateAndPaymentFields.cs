using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cabinet.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateAndPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "arret_debut",
                table: "Consultation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "arret_fin",
                table: "Consultation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "arret_jours",
                table: "Consultation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "arret_motif",
                table: "Consultation",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "certificat_aptitude",
                table: "Consultation",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "certificat_observation",
                table: "Consultation",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "payment_date",
                table: "Consultation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "Consultation",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receipt_number",
                table: "Consultation",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "arret_debut",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "arret_fin",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "arret_jours",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "arret_motif",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "certificat_aptitude",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "certificat_observation",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "payment_date",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "receipt_number",
                table: "Consultation");
        }
    }
}
