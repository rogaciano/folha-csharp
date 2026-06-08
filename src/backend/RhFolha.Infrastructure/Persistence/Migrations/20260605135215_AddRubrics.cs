using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRubrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rubrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    esocial_nature = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    allows_manual_entry = table.Column<bool>(type: "boolean", nullable: false),
                    allows_mass_entry = table.Column<bool>(type: "boolean", nullable: false),
                    allows_fixed_entry = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubrics", x => x.id);
                    table.ForeignKey(
                        name: "FK_rubrics_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rubric_validities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_on = table.Column<DateOnly>(type: "date", nullable: true),
                    incidence_inss = table.Column<bool>(type: "boolean", nullable: false),
                    incidence_fgts = table.Column<bool>(type: "boolean", nullable: false),
                    incidence_irrf = table.Column<bool>(type: "boolean", nullable: false),
                    incidence_dsr = table.Column<bool>(type: "boolean", nullable: false),
                    calculation_method = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    calculation_base = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubric_validities", x => x.id);
                    table.ForeignKey(
                        name: "FK_rubric_validities_rubrics_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "rubrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "rubrics",
                columns: new[] { "id", "allows_fixed_entry", "allows_manual_entry", "allows_mass_entry", "code", "company_id", "created_at", "description", "esocial_nature", "is_active", "name", "type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01"), false, false, false, "001", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "1000", true, "Salario mensal", "provento", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02"), false, true, true, "002", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "1000", true, "Producao", "provento", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03"), false, true, true, "101", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Adiantamento salarial", "provento", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04"), false, true, true, "201", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Desconto de adiantamento", "desconto", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05"), true, true, true, "202", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Vale transporte", "desconto", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06"), true, true, true, "203", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Plano de saude", "desconto", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee07"), false, false, false, "901", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "INSS", "desconto", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee08"), false, false, false, "902", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "IRRF", "desconto", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee09"), false, false, false, "903", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "FGTS informativo", "informativa", null }
                });

            migrationBuilder.InsertData(
                table: "rubric_validities",
                columns: new[] { "id", "calculation_base", "calculation_method", "created_at", "ends_on", "incidence_dsr", "incidence_fgts", "incidence_inss", "incidence_irrf", "is_active", "rubric_id", "starts_on", "updated_at" },
                values: new object[,]
                {
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff01"), "salario_base", "sistema", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, true, true, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff02"), "producao", "quantidade_valor", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, true, true, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff03"), "nenhuma", "valor_fixo", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff04"), "nenhuma", "valor_fixo", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff05"), "nenhuma", "valor_fixo", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff06"), "nenhuma", "valor_fixo", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff07"), "base_inss", "sistema", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee07"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff08"), "base_irrf", "sistema", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee08"), new DateOnly(2026, 1, 1), null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffff09"), "base_fgts", "sistema", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, false, false, true, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee09"), new DateOnly(2026, 1, 1), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_rubric_validities_rubric_id_starts_on",
                table: "rubric_validities",
                columns: new[] { "rubric_id", "starts_on" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rubrics_company_id_code",
                table: "rubrics",
                columns: new[] { "company_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rubric_validities");

            migrationBuilder.DropTable(
                name: "rubrics");
        }
    }
}
