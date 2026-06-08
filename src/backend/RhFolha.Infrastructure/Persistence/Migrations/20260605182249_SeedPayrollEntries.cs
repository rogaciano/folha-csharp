using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPayrollEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "payroll_entries",
                columns: new[] { "id", "amount", "company_id", "created_at", "employee_id", "entry_date", "notes", "origin", "payroll_period_id", "quantity", "reference", "rubric_id", "status", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), 800m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"), new DateOnly(2026, 6, 15), "Adiantamento quinzenal", "manual", new Guid("99999999-9999-9999-9999-999999999901"), null, "ADV-06/2026", new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03"), "aprovado", null },
                    { new Guid("11111111-1111-1111-1111-111111111102"), 625m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd03"), new DateOnly(2026, 6, 20), "Producao parcial do mes", "manual", new Guid("99999999-9999-9999-9999-999999999901"), 50m, "PROD-06/2026", new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02"), "aprovado", null },
                    { new Guid("11111111-1111-1111-1111-111111111103"), 180m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"), new DateOnly(2026, 6, 30), "Desconto plano de saude", "manual", new Guid("99999999-9999-9999-9999-999999999901"), null, "SAUDE-06/2026", new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06"), "aprovado", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "payroll_entries",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "payroll_entries",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "payroll_entries",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"));
        }
    }
}
