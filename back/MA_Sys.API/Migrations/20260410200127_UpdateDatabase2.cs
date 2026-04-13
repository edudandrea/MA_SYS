using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalAlunos",
                table: "Planos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Planos_AcademiaId",
                table: "Planos",
                column: "AcademiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Planos_Academias_AcademiaId",
                table: "Planos",
                column: "AcademiaId",
                principalTable: "Academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planos_Academias_AcademiaId",
                table: "Planos");

            migrationBuilder.DropIndex(
                name: "IX_Planos_AcademiaId",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "TotalAlunos",
                table: "Planos");
        }
    }
}
