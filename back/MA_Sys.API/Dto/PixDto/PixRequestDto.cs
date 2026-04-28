using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.PixDto
{
    public class PixRequestDto
    {
        public decimal Valor { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Nome { get; set; } =  string.Empty;
        public string Cidade { get; set; } = string.Empty;
    }
}
