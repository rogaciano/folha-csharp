using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePhotoAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "employees",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "employee_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    responsible = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_events_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_events_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                column: "photo_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                column: "photo_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddd03"),
                column: "photo_url",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_employee_events_company_id_employee_id_event_date",
                table: "employee_events",
                columns: new[] { "company_id", "employee_id", "event_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_events_company_id_status",
                table: "employee_events",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_events_company_id_type",
                table: "employee_events",
                columns: new[] { "company_id", "type" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_events_employee_id",
                table: "employee_events",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_events");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "employees");
        }
    }
}
