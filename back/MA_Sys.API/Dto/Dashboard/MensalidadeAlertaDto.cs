namespace MA_Sys.API.Dto.Dashboard
{
    public class MensalidadeAlertaDto
    {
        public int AlunoId { get; set; }
        public string NomeAluno { get; set; } = string.Empty;
        public DateTime DataVencimento { get; set; }
        public int DiasParaVencimento { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
