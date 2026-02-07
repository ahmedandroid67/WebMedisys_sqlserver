using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cabinet.Migrations
{
    /// <inheritdoc />
    public partial class Phase2CorrectnessPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "stock",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "stock",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "services",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "services",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "rendezvous",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "rendezvous",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "patient",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "patient",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Consultation",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "id_service",
                table: "Consultation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Consultation",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.Sql(
                """
                UPDATE c
                SET c.created_at = ISNULL(c.date_consultation, c.created_at),
                    c.updated_at = GETUTCDATE()
                FROM Consultation c;

                UPDATE p
                SET p.created_at = ISNULL(h.min_consultation_date, p.created_at),
                    p.updated_at = GETUTCDATE()
                FROM patient p
                OUTER APPLY (
                    SELECT MIN(c.date_consultation) AS min_consultation_date
                    FROM Consultation c
                    WHERE c.patient = p.id_patient
                ) h;

                UPDATE r
                SET r.created_at = ISNULL(r.dateheure, r.created_at),
                    r.updated_at = GETUTCDATE()
                FROM rendezvous r;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_rendezvous_dateheure",
                table: "rendezvous",
                column: "dateheure");

            migrationBuilder.CreateIndex(
                name: "IX_Consultation_date_consultation_etat_patient",
                table: "Consultation",
                columns: new[] { "date_consultation", "etat", "patient" });

            migrationBuilder.CreateIndex(
                name: "IX_Consultation_id_service",
                table: "Consultation",
                column: "id_service");

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_stock_CategoryId' AND object_id = OBJECT_ID(N'[stock]'))
                    CREATE INDEX [IX_stock_CategoryId] ON [stock] ([CategoryId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_stock_movements_StockId' AND object_id = OBJECT_ID(N'[stock_movements]'))
                    CREATE INDEX [IX_stock_movements_StockId] ON [stock_movements] ([StockId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_stock_movements_EmployerId' AND object_id = OBJECT_ID(N'[stock_movements]'))
                    CREATE INDEX [IX_stock_movements_EmployerId] ON [stock_movements] ([EmployerId]);
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultation_services_id_service",
                table: "Consultation",
                column: "id_service",
                principalTable: "services",
                principalColumn: "id_service",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultation_services_id_service",
                table: "Consultation");

            migrationBuilder.DropIndex(
                name: "IX_rendezvous_dateheure",
                table: "rendezvous");

            migrationBuilder.DropIndex(
                name: "IX_Consultation_date_consultation_etat_patient",
                table: "Consultation");

            migrationBuilder.DropIndex(
                name: "IX_Consultation_id_service",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "stock");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "stock");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "services");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "services");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "rendezvous");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "rendezvous");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "patient");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "patient");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "id_service",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Consultation");
        }
    }
}
