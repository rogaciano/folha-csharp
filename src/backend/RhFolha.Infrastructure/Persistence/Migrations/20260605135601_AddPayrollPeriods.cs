using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payroll_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_on = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_payroll_periods_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "payroll_periods",
                columns: new[] { "id", "closed_at", "code", "company_id", "created_at", "ends_on", "month", "opened_at", "starts_on", "status", "updated_at", "year" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999901"), null, "2026-06", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 6, 30), 6, new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 6, 1), "aberta", null, 2026 });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_periods_company_id_code",
                table: "payroll_periods",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_periods_company_id_year_month",
                table: "payroll_periods",
                columns: new[] { "company_id", "year", "month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payroll_periods");
        }
    }
}
