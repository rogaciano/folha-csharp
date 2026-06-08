using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatutoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "statutory_tables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_on = table.Column<DateOnly>(type: "date", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statutory_tables", x => x.id);
                    table.ForeignKey(
                        name: "FK_statutory_tables_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "statutory_table_ranges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    statutory_table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lower_limit = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    upper_limit = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    rate_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    deduction_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statutory_table_ranges", x => x.id);
                    table.ForeignKey(
                        name: "FK_statutory_table_ranges_statutory_tables_statutory_table_id",
                        column: x => x.statutory_table_id,
                        principalTable: "statutory_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "statutory_tables",
                columns: new[] { "id", "company_id", "created_at", "ends_on", "is_active", "name", "notes", "starts_on", "type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INSS demonstracao 2026", "Valores demonstrativos para desenvolvimento. Revisar tabela oficial antes de producao.", new DateOnly(2026, 1, 1), "inss", null },
                    { new Guid("33333333-3333-3333-3333-333333333302"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "IRRF demonstracao 2026", "Valores demonstrativos para desenvolvimento. Revisar tabela oficial antes de producao.", new DateOnly(2026, 1, 1), "irrf", null }
                });

            migrationBuilder.InsertData(
                table: "statutory_table_ranges",
                columns: new[] { "id", "created_at", "deduction_amount", "lower_limit", "rate_percent", "statutory_table_id", "updated_at", "upper_limit" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444401"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0m, 7.5m, new Guid("33333333-3333-3333-3333-333333333301"), null, 1518.00m },
                    { new Guid("44444444-4444-4444-4444-444444444402"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 22.77m, 1518.01m, 9.0m, new Guid("33333333-3333-3333-3333-333333333301"), null, 2793.88m },
                    { new Guid("44444444-4444-4444-4444-444444444403"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 106.59m, 2793.89m, 12.0m, new Guid("33333333-3333-3333-3333-333333333301"), null, 4190.83m },
                    { new Guid("44444444-4444-4444-4444-444444444404"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 190.40m, 4190.84m, 14.0m, new Guid("33333333-3333-3333-3333-333333333301"), null, 8157.41m },
                    { new Guid("44444444-4444-4444-4444-444444444405"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0m, 0m, new Guid("33333333-3333-3333-3333-333333333302"), null, 2259.20m },
                    { new Guid("44444444-4444-4444-4444-444444444406"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 169.44m, 2259.21m, 7.5m, new Guid("33333333-3333-3333-3333-333333333302"), null, 2826.65m },
                    { new Guid("44444444-4444-4444-4444-444444444407"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 381.44m, 2826.66m, 15.0m, new Guid("33333333-3333-3333-3333-333333333302"), null, 3751.05m },
                    { new Guid("44444444-4444-4444-4444-444444444408"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 662.77m, 3751.06m, 22.5m, new Guid("33333333-3333-3333-3333-333333333302"), null, 4664.68m },
                    { new Guid("44444444-4444-4444-4444-444444444409"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), 896.00m, 4664.69m, 27.5m, new Guid("33333333-3333-3333-3333-333333333302"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_statutory_table_ranges_statutory_table_id_lower_limit",
                table: "statutory_table_ranges",
                columns: new[] { "statutory_table_id", "lower_limit" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_statutory_tables_company_id_is_active",
                table: "statutory_tables",
                columns: new[] { "company_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_statutory_tables_company_id_type_starts_on",
                table: "statutory_tables",
                columns: new[] { "company_id", "type", "starts_on" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "statutory_table_ranges");

            migrationBuilder.DropTable(
                name: "statutory_tables");
        }
    }
}
