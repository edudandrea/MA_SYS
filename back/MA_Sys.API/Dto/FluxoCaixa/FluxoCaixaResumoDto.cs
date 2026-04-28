namespace MA_Sys.API.Dto.FluxoCaixa
{
    public class FluxoCaixaResumoDto
    {
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal Saldo { get; set; }
        public int TotalMovimentos { get; set; }
    }
}
