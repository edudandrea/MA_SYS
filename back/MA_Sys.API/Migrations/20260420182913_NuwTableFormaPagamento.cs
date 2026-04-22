using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MA_SYS.Api.Migrations
{
    /// <inheritdoc />
    public partial class NuwTableFormaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormaPagamentoId",
                table: "Matriculas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPagamentoId",
                table: "Matriculas");
        }
    }
}
