using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MA_Sys.API.Models;

namespace MA_SYS.Api.Models
{
    public class Pagamentos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int PlanoId { get; set; }
        public int AlunoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataVencimento { get; set; }
        public string Status { get; set; } = "Pendente";        
        public Aluno? Aluno { get; set; }
        public Plano? Plano { get; set; }
        public int FormaPagamentoId { get; set; }
        public FormaPagamento? FormaPagamento { get; set; }
    }
}