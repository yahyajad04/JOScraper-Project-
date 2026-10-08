using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoSale.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Items",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreItem_StoreId",
                table: "StoreItem",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_NormalizedName",
                table: "Items",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreItem_StoreId",
                table: "StoreItem");

            migrationBuilder.DropIndex(
                name: "IX_Items_NormalizedName",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Items");
        }
    }
}
