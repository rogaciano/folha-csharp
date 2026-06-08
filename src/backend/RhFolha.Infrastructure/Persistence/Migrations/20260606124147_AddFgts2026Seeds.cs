using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFgts2026Seeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "statutory_tables",
                columns: new[] { "id", "company_id", "created_at", "ends_on", "is_active", "name", "notes", "starts_on", "type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333304"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "FGTS mensal 2026", "Deposito mensal de FGTS a cargo do empregador: 8% da remuneracao paga ou devida ao trabalhador CLT.", new DateOnly(2026, 1, 1), "fgts", null },
                    { new Guid("33333333-3333-3333-3333-333333333305"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "FGTS saque-aniversario 2026", "Tabela oficial de saque-aniversario para consulta. Nao compoe o calculo mensal da folha do empregador.", new DateOnly(2026, 1, 1), "fgts_saque_aniversario", null }
                });

            migrationBuilder.InsertData(
                table: "statutory_table_ranges",
                columns: new[] { "id", "created_at", "deduction_amount", "lower_limit", "rate_percent", "statutory_table_id", "updated_at", "upper_limit" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444412"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0m, 8.0m, new Guid("33333333-3333-3333-3333-333333333304"), null, null },
                    { new Guid("44444444-4444-4444-4444-444444444413"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0.01m, 50.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, 500.00m },
                    { new Guid("44444444-4444-4444-4444-444444444414"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 50.00m, 500.01m, 40.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, 1000.00m },
                    { new Guid("44444444-4444-4444-4444-444444444415"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 150.00m, 1000.01m, 30.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, 5000.00m },
                    { new Guid("44444444-4444-4444-4444-444444444416"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 650.00m, 5000.01m, 20.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, 10000.00m },
                    { new Guid("44444444-4444-4444-4444-444444444417"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 1150.00m, 10000.01m, 15.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, 15000.00m },
                    { new Guid("44444444-4444-4444-4444-444444444418"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 1900.00m, 15000.01m, 10.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, 20000.00m },
                    { new Guid("44444444-4444-4444-4444-444444444419"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 2900.00m, 20000.01m, 5.0m, new Guid("33333333-3333-3333-3333-333333333305"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444412"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444413"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444414"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444415"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444416"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444417"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444418"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444419"));

            migrationBuilder.DeleteData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333304"));

            migrationBuilder.DeleteData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333305"));
        }
    }
}
