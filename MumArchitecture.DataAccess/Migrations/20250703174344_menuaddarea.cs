using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MumArchitecture.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class menuaddarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methods_Roles_RoleId",
                table: "Methods");

            migrationBuilder.DropIndex(
                name: "IX_Methods_RoleId",
                table: "Methods");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Methods");

            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Menus");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Methods",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Methods_RoleId",
                table: "Methods",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methods_Roles_RoleId",
                table: "Methods",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
