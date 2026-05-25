namespace MA_Sys.API.Dto.Turmas
{
    public class TurmaAlunoDto
    {
        public int AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool CheckInProximaAula { get; set; }
        public DateTime? DataCheckInProximaAula { get; set; }
        public string? DiaSemanaCheckIn { get; set; }
        public List<TurmaAlunoCheckInDto> CheckInsAulas { get; set; } = [];
    }

    public class TurmaAlunoCheckInDto
    {
        public int CheckInId { get; set; }
        public DateTime DataAula { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
    }
}
