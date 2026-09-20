using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynd.Api.Migrations
{
    /// <inheritdoc />
    public partial class addcategoryrelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "LostItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "FoundItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LostItems_CategoryId",
                table: "LostItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FoundItems_CategoryId",
                table: "FoundItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoundItems_Category_CategoryId",
                table: "FoundItems",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LostItems_Category_CategoryId",
                table: "LostItems",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoundItems_Category_CategoryId",
                table: "FoundItems");

            migrationBuilder.DropForeignKey(
                name: "FK_LostItems_Category_CategoryId",
                table: "LostItems");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_LostItems_CategoryId",
                table: "LostItems");

            migrationBuilder.DropIndex(
                name: "IX_FoundItems_CategoryId",
                table: "FoundItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "LostItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "FoundItems");
        }
    }
}
