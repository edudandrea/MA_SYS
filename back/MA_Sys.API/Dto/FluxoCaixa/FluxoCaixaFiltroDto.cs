namespace MA_Sys.API.Dto.FluxoCaixa
{
    public class FluxoCaixaFiltroDto
    {
        public int? AcademiaId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? Status { get; set; }
        public string? FormaPagamento { get; set; }
        public string? Descricao { get; set; }
    }
}
