using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Services
{
    public class PixService
    {

        private string LimparTextoPix(string texto)
        {
            return texto
                .ToUpper()
                .Replace("Á", "A")
                .Replace("À", "A")
                .Replace("Ã", "A")
                .Replace("Â", "A")
                .Replace("É", "E")
                .Replace("Ê", "E")
                .Replace("Í", "I")
                .Replace("Ó", "O")
                .Replace("Ô", "O")
                .Replace("Õ", "O")
                .Replace("Ú", "U")
                .Replace("Ç", "C")
                .Replace(" ", ""); // 🔥 REMOVE ESPAÇOS
        }
        private string GerarCRC16(string payload)
        {
            int polinomio = 0x1021;
            int resultado = 0xFFFF;

            var bytes = System.Text.Encoding.ASCII.GetBytes(payload);

            foreach (var b in bytes)
            {
                resultado ^= (b << 8);

                for (int i = 0; i < 8; i++)
                {
                    if ((resultado & 0x8000) != 0)
                        resultado = (resultado << 1) ^ polinomio;
                    else
                        resultado <<= 1;
                }
            }

            return (resultado & 0xFFFF).ToString("X4");
        }
        public string GerarPixPayload(string chavePix, string nome, string cidade, decimal valor)
        {
            nome = LimparTextoPix(nome);
            cidade = LimparTextoPix(cidade);

            string valorFormatado = valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            // 🔥 BLOCO PIX (CORRETO)
            string merchantAccount =
                "0014BR.GOV.BCB.PIX" +
                $"01{chavePix.Length:D2}{chavePix}";

            string payload =
                "000201" +
                "010212" +
                $"26{merchantAccount.Length:D2}{merchantAccount}" + // 🔥 AQUI ESTAVA O ERRO
                "52040000" +
                "5303986" +
                $"54{valorFormatado.Length:D2}{valorFormatado}" +
                "5802BR" +
                $"59{nome.Length:D2}{nome}" +
                $"60{cidade.Length:D2}{cidade}" +
                "62070503***" +
                "6304";

            string crc = GerarCRC16(payload);

            return payload + crc;
        }
    }
}