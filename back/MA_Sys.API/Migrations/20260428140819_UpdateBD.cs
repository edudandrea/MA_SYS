using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChavePix",
                table: "Academias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoAccessToken",
                table: "Academias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPublicKey",
                table: "Academias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PagamentosAcademias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AcademiaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosAcademias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentosAcademias_Academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "Academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosAcademias_AcademiaId",
                table: "PagamentosAcademias",
                column: "AcademiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagamentosAcademias");

            migrationBuilder.DropColumn(
                name: "ChavePix",
                table: "Academias");

            migrationBuilder.DropColumn(
                name: "MercadoPagoAccessToken",
                table: "Academias");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPublicKey",
                table: "Academias");
        }
    }
}
