namespace MA_Sys.API.Dto.Dashboard
{
    public class AcademiaPagamentoAlertaDto
    {
        public int AcademiaId { get; set; }
        public string NomeAcademia { get; set; } = string.Empty;
        public int TotalPagamentosPendentes { get; set; }
        public int TotalPagamentosVencidos { get; set; }
        public string StatusGeral { get; set; } = "EmDia";
    }
}
