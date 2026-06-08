using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeResponsible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "responsible_employee_id",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                column: "responsible_employee_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                column: "responsible_employee_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddd03"),
                column: "responsible_employee_id",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_employees_company_id_responsible_employee_id",
                table: "employees",
                columns: new[] { "company_id", "responsible_employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_responsible_employee_id",
                table: "employees",
                column: "responsible_employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employees_employees_responsible_employee_id",
                table: "employees",
                column: "responsible_employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_employees_responsible_employee_id",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_company_id_responsible_employee_id",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_responsible_employee_id",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "responsible_employee_id",
                table: "employees");
        }
    }
}
