using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaWebPapeleria.Migrations
{
    /// <inheritdoc />
    public partial class agregoIsOpenenCashClosing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "CashClosings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "CashClosings");
        }
    }
}
