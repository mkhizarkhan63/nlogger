using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class update_exceptionClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Exceptions",
                table: "Exception",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Linenumber",
                table: "Exception",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exceptions",
                table: "Exception");

            migrationBuilder.DropColumn(
                name: "Linenumber",
                table: "Exception");
        }
    }
}
