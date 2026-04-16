using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_Sys.API.Models
{
    public class Financeiro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int? AlunoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string TIpo { get; set; }  = string.Empty;
        public int FormaPagamentoId { get; set; }
        public FormaPagamento? FormaPagamento { get; set; }
        public int CategoriaId { get; set; }
        public CategoriaTransacao? Categoria { get; set; }
        public string Descrição { get; set; } = string.Empty;
        public bool Pago { get; set; }
    }
}