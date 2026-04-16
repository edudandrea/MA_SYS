using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Pagamentos");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Pagamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormaPagamentoId",
                table: "Pagamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NomeModalidade",
                table: "Modalidades",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FormaPagamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AcademiaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Taxa = table.Column<decimal>(type: "TEXT", nullable: false),
                    Parcelas = table.Column<int>(type: "INTEGER", nullable: false),
                    Dias = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormaPagamentos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_FormaPagamentoId",
                table: "Pagamentos",
                column: "FormaPagamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamentos_FormaPagamentos_FormaPagamentoId",
                table: "Pagamentos",
                column: "FormaPagamentoId",
                principalTable: "FormaPagamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagamentos_FormaPagamentos_FormaPagamentoId",
                table: "Pagamentos");

            migrationBuilder.DropTable(
                name: "FormaPagamentos");

            migrationBuilder.DropIndex(
                name: "IX_Pagamentos_FormaPagamentoId",
                table: "Pagamentos");

            migrationBuilder.DropColumn(
                name: "FormaPagamentoId",
                table: "Pagamentos");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Pagamentos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamento",
                table: "Pagamentos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NomeModalidade",
                table: "Modalidades",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
