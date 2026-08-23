using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPImobiliaria.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDestaque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmDestaque",
                table: "Imoveis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmDestaque",
                table: "Imoveis");
        }
    }
}
