using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Matricula
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AcademiaId { get; set; }

        public int AlunoId { get; set; }

        public int PlanoId { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }
        public string Status { get; set; } = string.Empty;
        public Aluno Aluno { get; set; } = null!;
        public Plano Plano { get; set; } = null!;
    }
}