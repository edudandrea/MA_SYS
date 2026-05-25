namespace MA_Sys.API.Dto.Turmas
{
    public class TurmaAlunoDto
    {
        public int AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool CheckInProximaAula { get; set; }
        public DateTime? DataCheckInProximaAula { get; set; }
        public string? DiaSemanaCheckIn { get; set; }
    }
}
