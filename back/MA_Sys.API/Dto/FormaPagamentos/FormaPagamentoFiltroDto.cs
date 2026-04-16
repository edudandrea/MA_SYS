using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.FormaPagamentos
{
    public class FormaPagamentoFiltroDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}