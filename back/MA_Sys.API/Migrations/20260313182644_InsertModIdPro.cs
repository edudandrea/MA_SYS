using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class InsertModIdPro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "totalAlunos",
                table: "Modalidades",
                newName: "TotalAlunos");

            migrationBuilder.AddColumn<int>(
                name: "ModalidadeId",
                table: "Professores",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademiaId",
                table: "Modalidades",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademiaId",
                table: "Alunos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModalidadeId",
                table: "Professores");

            migrationBuilder.DropColumn(
                name: "AcademiaId",
                table: "Modalidades");

            migrationBuilder.DropColumn(
                name: "AcademiaId",
                table: "Alunos");

            migrationBuilder.RenameColumn(
                name: "TotalAlunos",
                table: "Modalidades",
                newName: "totalAlunos");
        }
    }
}
