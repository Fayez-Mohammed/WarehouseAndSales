using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SupplierAsAspNetUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Suppliers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_UserId",
                table: "Suppliers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_AspNetUsers_UserId",
                table: "Suppliers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_AspNetUsers_UserId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_UserId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Suppliers");
        }
    }
}
