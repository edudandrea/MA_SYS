namespace MA_Sys.API.Dto.FluxoCaixa
{
    public class FluxoCaixaResponseDto
    {
        public FluxoCaixaResumoDto Resumo { get; set; } = new();
        public List<FluxoCaixaMovimentoDto> Movimentos { get; set; } = [];
    }
}
