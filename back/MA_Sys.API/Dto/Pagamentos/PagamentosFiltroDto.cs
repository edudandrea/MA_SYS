namespace MA_Sys.API.Dto.Pagamentos
{
    public class PagamentosFiltroDto
    {
        public int Id { get; set; }
        public string? FormaPagamento { get; set; }
        public int AlunoId { get; set; }
        public decimal Valor { get; set; }

    }
}