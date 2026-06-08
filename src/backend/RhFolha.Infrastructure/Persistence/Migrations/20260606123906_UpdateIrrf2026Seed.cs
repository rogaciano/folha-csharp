using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIrrf2026Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444405"),
                column: "upper_limit",
                value: 2428.80m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444406"),
                columns: new[] { "deduction_amount", "lower_limit" },
                values: new object[] { 182.16m, 2428.81m });

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444407"),
                column: "deduction_amount",
                value: 394.16m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444408"),
                column: "deduction_amount",
                value: 675.49m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444409"),
                column: "deduction_amount",
                value: 908.73m);

            migrationBuilder.UpdateData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333302"),
                columns: new[] { "name", "notes" },
                values: new object[] { "IRRF 2026", "Tabela progressiva mensal publicada pela Receita Federal para fatos geradores a partir de janeiro de 2026." });

            migrationBuilder.InsertData(
                table: "statutory_tables",
                columns: new[] { "id", "company_id", "created_at", "ends_on", "is_active", "name", "notes", "starts_on", "type", "updated_at" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333303"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "IRRF reducao 2026", "Regra redutora da Lei 15.270/2025: ate R$ 5.000,00 reduz ate R$ 312,89; de R$ 5.000,01 a R$ 7.350,00 aplica reducao = R$ 978,62 - 0,133145 x rendimentos tributaveis mensais.", new DateOnly(2026, 1, 1), "irrf_reducao", null });

            migrationBuilder.InsertData(
                table: "statutory_table_ranges",
                columns: new[] { "id", "created_at", "deduction_amount", "lower_limit", "rate_percent", "statutory_table_id", "updated_at", "upper_limit" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444410"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 312.89m, 0m, 0m, new Guid("33333333-3333-3333-3333-333333333303"), null, 5000.00m },
                    { new Guid("44444444-4444-4444-4444-444444444411"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 978.62m, 5000.01m, 13.3145m, new Guid("33333333-3333-3333-3333-333333333303"), null, 7350.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444410"));

            migrationBuilder.DeleteData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444411"));

            migrationBuilder.DeleteData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333303"));

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444405"),
                column: "upper_limit",
                value: 2259.20m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444406"),
                columns: new[] { "deduction_amount", "lower_limit" },
                values: new object[] { 169.44m, 2259.21m });

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444407"),
                column: "deduction_amount",
                value: 381.44m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444408"),
                column: "deduction_amount",
                value: 662.77m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444409"),
                column: "deduction_amount",
                value: 896.00m);

            migrationBuilder.UpdateData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333302"),
                columns: new[] { "name", "notes" },
                values: new object[] { "IRRF demonstracao 2026", "Valores demonstrativos para desenvolvimento. Revisar tabela oficial antes de producao." });
        }
    }
}
