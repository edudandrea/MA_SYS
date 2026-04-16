namespace MA_Sys.API.Dto.Planos
{
    public class PlanosUpdateDto
    {
        
        public string? Nome { get; set; }
        public decimal Valor { get; set; }
        public int DuracaoMeses { get; set; }
        public bool Ativo { get; set; }
    }
}