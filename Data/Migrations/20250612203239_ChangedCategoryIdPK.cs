using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZaiEats.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedCategoryIdPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_CategoryId",
                table: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "MenuItems",
                newName: "MenuCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                newName: "IX_MenuItems_MenuCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_MenuCategoryId",
                table: "MenuItems",
                column: "MenuCategoryId",
                principalTable: "MenuCategories",
                principalColumn: "MenuCategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "MenuCategoryId",
                table: "MenuItems",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_MenuCategoryId",
                table: "MenuItems",
                newName: "IX_MenuItems_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_CategoryId",
                table: "MenuItems",
                column: "CategoryId",
                principalTable: "MenuCategories",
                principalColumn: "MenuCategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
