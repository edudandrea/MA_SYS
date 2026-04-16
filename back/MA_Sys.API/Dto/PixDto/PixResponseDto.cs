using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.PixDto
{
    public class PixResponseDto
    {
        public string Payload { get; set; } = string.Empty;
        public string QrCodeBase64 { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }
}