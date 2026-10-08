using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoSale.Migrations
{
    /// <inheritdoc />
    public partial class ImageURLAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "StoreItem",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "StoreItem");
        }
    }
}
