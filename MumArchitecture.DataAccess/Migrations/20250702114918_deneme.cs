using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MumArchitecture.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class deneme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
