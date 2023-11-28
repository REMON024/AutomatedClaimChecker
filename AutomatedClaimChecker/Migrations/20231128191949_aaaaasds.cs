using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedClaimChecker.Migrations
{
    public partial class aaaaasds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RequiredDocumentAccuracy",
                table: "DocumentTypes",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "RequiredDocumentAccuracy",
                table: "DocumentTypes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
