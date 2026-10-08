using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoSale.Migrations
{
    /// <inheritdoc />
    public partial class AddVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Vector",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vector",
                table: "Categories");
        }
    }
}
