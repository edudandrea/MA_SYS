namespace MA_Sys.API.Dto.Alunos
{
    public class AlunoCheckInPublicoDto
    {
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TurmaId { get; set; }
    }
}
