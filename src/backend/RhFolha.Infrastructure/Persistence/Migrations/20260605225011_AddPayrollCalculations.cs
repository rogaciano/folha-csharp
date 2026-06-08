using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollCalculations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payroll_calculations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_proventos = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    total_descontos = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    total_liquido = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    employee_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_calculations", x => x.id);
                    table.ForeignKey(
                        name: "FK_payroll_calculations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_calculations_payroll_periods_payroll_period_id",
                        column: x => x.payroll_period_id,
                        principalTable: "payroll_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_calculation_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_calculation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_registration = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    employee_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rubric_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    rubric_name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    rubric_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    origin = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_calculation_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_payroll_calculation_items_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_calculation_items_payroll_calculations_payroll_calc~",
                        column: x => x.payroll_calculation_id,
                        principalTable: "payroll_calculations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payroll_calculation_items_rubrics_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "rubrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calculation_items_employee_id",
                table: "payroll_calculation_items",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calculation_items_payroll_calculation_id_employee_id",
                table: "payroll_calculation_items",
                columns: new[] { "payroll_calculation_id", "employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calculation_items_payroll_calculation_id_rubric_id",
                table: "payroll_calculation_items",
                columns: new[] { "payroll_calculation_id", "rubric_id" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calculation_items_rubric_id",
                table: "payroll_calculation_items",
                column: "rubric_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calculations_company_id_payroll_period_id_is_current",
                table: "payroll_calculations",
                columns: new[] { "company_id", "payroll_period_id", "is_current" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calculations_payroll_period_id",
                table: "payroll_calculations",
                column: "payroll_period_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payroll_calculation_items");

            migrationBuilder.DropTable(
                name: "payroll_calculations");
        }
    }
}
