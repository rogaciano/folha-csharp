using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedPayrollEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fixed_payroll_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_on = table.Column<DateOnly>(type: "date", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fixed_payroll_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_fixed_payroll_entries_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fixed_payroll_entries_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fixed_payroll_entries_rubrics_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "rubrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "fixed_payroll_entries",
                columns: new[] { "id", "amount", "company_id", "created_at", "employee_id", "ends_on", "is_active", "notes", "quantity", "rubric_id", "starts_on", "updated_at" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), 180m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"), null, true, "Plano de saude mensal", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06"), new DateOnly(2026, 6, 1), null },
                    { new Guid("22222222-2222-2222-2222-222222222202"), 192m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"), null, true, "Vale transporte mensal", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05"), new DateOnly(2026, 6, 1), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_fixed_payroll_entries_company_id_employee_id",
                table: "fixed_payroll_entries",
                columns: new[] { "company_id", "employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_fixed_payroll_entries_company_id_is_active",
                table: "fixed_payroll_entries",
                columns: new[] { "company_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_fixed_payroll_entries_company_id_rubric_id",
                table: "fixed_payroll_entries",
                columns: new[] { "company_id", "rubric_id" });

            migrationBuilder.CreateIndex(
                name: "IX_fixed_payroll_entries_employee_id",
                table: "fixed_payroll_entries",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_fixed_payroll_entries_rubric_id",
                table: "fixed_payroll_entries",
                column: "rubric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fixed_payroll_entries");
        }
    }
}
