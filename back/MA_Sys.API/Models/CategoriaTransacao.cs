using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_Sys.API.Models
{
    public class CategoriaTransacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string Nome { get; set; }  = string.Empty;
        public string TipoTransacao { get; set; } = string.Empty;
    }
}