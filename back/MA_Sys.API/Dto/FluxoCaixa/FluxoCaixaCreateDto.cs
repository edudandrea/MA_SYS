namespace MA_Sys.API.Dto.FluxoCaixa
{
    public class FluxoCaixaCreateDto
    {
        public int? AcademiaId { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int? FormaPagamentoId { get; set; }
        public bool Pago { get; set; } = true;
    }
}
