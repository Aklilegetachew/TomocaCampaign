using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TomocaCampaignAPI.Migrations
{
    /// <inheritdoc />
    public partial class EmployeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeCode",
                table: "Employees",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeCode",
                table: "Employees");
        }
    }
}
