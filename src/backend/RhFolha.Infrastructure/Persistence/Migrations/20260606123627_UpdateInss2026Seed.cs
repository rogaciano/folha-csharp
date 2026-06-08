using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInss2026Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444401"),
                column: "upper_limit",
                value: 1621.00m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444402"),
                columns: new[] { "deduction_amount", "lower_limit", "upper_limit" },
                values: new object[] { 24.32m, 1621.01m, 2902.84m });

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444403"),
                columns: new[] { "deduction_amount", "lower_limit", "upper_limit" },
                values: new object[] { 111.40m, 2902.85m, 4354.27m });

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444404"),
                columns: new[] { "deduction_amount", "lower_limit", "upper_limit" },
                values: new object[] { 198.49m, 4354.28m, 8475.55m });

            migrationBuilder.UpdateData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333301"),
                columns: new[] { "name", "notes" },
                values: new object[] { "INSS 2026", "Tabela informada para parametrizacao de 2026. Teto: R$ 8.475,55. Revisar a fonte oficial antes de uso em producao." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444401"),
                column: "upper_limit",
                value: 1518.00m);

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444402"),
                columns: new[] { "deduction_amount", "lower_limit", "upper_limit" },
                values: new object[] { 22.77m, 1518.01m, 2793.88m });

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444403"),
                columns: new[] { "deduction_amount", "lower_limit", "upper_limit" },
                values: new object[] { 106.59m, 2793.89m, 4190.83m });

            migrationBuilder.UpdateData(
                table: "statutory_table_ranges",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444404"),
                columns: new[] { "deduction_amount", "lower_limit", "upper_limit" },
                values: new object[] { 190.40m, 4190.84m, 8157.41m });

            migrationBuilder.UpdateData(
                table: "statutory_tables",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333301"),
                columns: new[] { "name", "notes" },
                values: new object[] { "INSS demonstracao 2026", "Valores demonstrativos para desenvolvimento. Revisar tabela oficial antes de producao." });
        }
    }
}
