using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cabinet.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationIdToOrdonnance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ordonnances_PatientID",
                table: "Ordonnances");

            migrationBuilder.AddColumn<int>(
                name: "ConsultationID",
                table: "Ordonnances",
                type: "int",
                nullable: true);

            // Backfill: Link existing ordonnances to consultations by patient+date match
            migrationBuilder.Sql(@"
                UPDATE o
                SET o.ConsultationID = c.id_consultation
                FROM Ordonnances o
                INNER JOIN Consultation c
                    ON c.patient = o.PatientID
                    AND CAST(c.date_consultation AS DATE) = CAST(o.DatePrescription AS DATE)
                WHERE o.ConsultationID IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ordonnances_ConsultationID",
                table: "Ordonnances",
                column: "ConsultationID");

            migrationBuilder.CreateIndex(
                name: "IX_Ordonnances_PatientID_DatePrescription",
                table: "Ordonnances",
                columns: new[] { "PatientID", "DatePrescription" });

            migrationBuilder.AddForeignKey(
                name: "FK_Ordonnances_Consultation_ConsultationID",
                table: "Ordonnances",
                column: "ConsultationID",
                principalTable: "Consultation",
                principalColumn: "id_consultation",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ordonnances_Consultation_ConsultationID",
                table: "Ordonnances");

            migrationBuilder.DropIndex(
                name: "IX_Ordonnances_ConsultationID",
                table: "Ordonnances");

            migrationBuilder.DropIndex(
                name: "IX_Ordonnances_PatientID_DatePrescription",
                table: "Ordonnances");

            migrationBuilder.DropColumn(
                name: "ConsultationID",
                table: "Ordonnances");

            migrationBuilder.CreateIndex(
                name: "IX_Ordonnances_PatientID",
                table: "Ordonnances",
                column: "PatientID");
        }
    }
}
