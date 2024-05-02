using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectDotNet.Migrations
{
    /// <inheritdoc />
    public partial class Migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddToCartDetails_Products_carId",
                table: "AddToCartDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_categoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Products_carId",
                table: "Purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Cars");

            migrationBuilder.RenameIndex(
                name: "IX_Products_categoryId",
                table: "Cars",
                newName: "IX_Cars_categoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cars",
                table: "Cars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AddToCartDetails_Cars_carId",
                table: "AddToCartDetails",
                column: "carId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_Categories_categoryId",
                table: "Cars",
                column: "categoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Cars_carId",
                table: "Purchase",
                column: "carId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddToCartDetails_Cars_carId",
                table: "AddToCartDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Cars_Categories_categoryId",
                table: "Cars");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Cars_carId",
                table: "Purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cars",
                table: "Cars");

            migrationBuilder.RenameTable(
                name: "Cars",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Cars_categoryId",
                table: "Products",
                newName: "IX_Products_categoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AddToCartDetails_Products_carId",
                table: "AddToCartDetails",
                column: "carId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_categoryId",
                table: "Products",
                column: "categoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Products_carId",
                table: "Purchase",
                column: "carId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
