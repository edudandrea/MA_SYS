using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.Pagamentos
{
    public class PagamentoRegistroDto
    {
        public int AlunoId { get; set; }
        public int PlanoId { get; set; }
        public int AcademiaId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataVencimento { get; set; }
        public string? Status { get; set; }
        public string? FormaPagamento { get; set; }
    }
}