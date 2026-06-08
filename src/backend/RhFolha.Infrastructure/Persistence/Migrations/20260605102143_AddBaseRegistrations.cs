using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    internal_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                    table.ForeignKey(
                        name: "FK_departments_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "job_positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    internal_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cbo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_positions_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    document_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    admission_date = table.Column<DateOnly>(type: "date", nullable: false),
                    compensation_model = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    base_salary = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    production_unit_value = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_employees_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_job_positions_job_position_id",
                        column: x => x.job_position_id,
                        principalTable: "job_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "id", "created_at", "document_number", "is_active", "legal_name", "trade_name", "updated_at" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "12345678000190", true, "ConsulCLT Demonstracao Ltda", "ConsulCLT", null });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "id", "company_id", "created_at", "internal_code", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "OPER", true, "Operacoes", null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "ADM", true, "Administrativo", null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "PROD", true, "Producao", null }
                });

            migrationBuilder.InsertData(
                table: "job_positions",
                columns: new[] { "id", "cbo", "company_id", "created_at", "internal_code", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"), "142105", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "GER-OPER", true, "Gerente de Operacoes", null },
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc2"), "411010", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "ASS-ADM", true, "Assistente Administrativo", null },
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc3"), "763210", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "COST-IND", true, "Costureira Industrial", null }
                });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "id", "admission_date", "base_salary", "company_id", "compensation_model", "created_at", "department_id", "document_number", "job_position_id", "name", "production_unit_value", "registration", "status", "updated_at" },
                values: new object[,]
                {
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"), new DateOnly(2022, 11, 1), 9500.00m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "mensalista", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), "78912345608", new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"), "Roberto de Souza Carvalho", 0m, "0001", "Active", null },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"), new DateOnly(2025, 1, 20), 3200.00m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "mensalista", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), "45678912304", new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc2"), "Mariana Costa Santos", 0m, "0002", "Active", null },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddd03"), new DateOnly(2023, 8, 10), 0m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "producao", new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"), "98765432100", new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc3"), "Ana Beatris Oliveira", 12.50m, "0003", "Active", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_departments_company_id_internal_code",
                table: "departments",
                columns: new[] { "company_id", "internal_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_company_id_document_number",
                table: "employees",
                columns: new[] { "company_id", "document_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_company_id_registration",
                table: "employees",
                columns: new[] { "company_id", "registration" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_department_id",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_job_position_id",
                table: "employees",
                column: "job_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_positions_company_id_internal_code",
                table: "job_positions",
                columns: new[] { "company_id", "internal_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "job_positions");

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        }
    }
}
