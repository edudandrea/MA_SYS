using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class CheckInAula
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int AlunoId { get; set; }
        public int TurmaId { get; set; }
        public DateTime DataCheckIn { get; set; }
        public string Origem { get; set; } = "PortalAluno";
        public Aluno? Aluno { get; set; }
        public Turma? Turma { get; set; }
    }
}
