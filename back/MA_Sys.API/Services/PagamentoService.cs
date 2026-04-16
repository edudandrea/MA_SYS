using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Pagamentos;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class PagamentoService
    {
        private readonly IPlanosRepository _planoRepo;
        private readonly IPagamentoRepository _pagRepo;
        public PagamentoService(IPlanosRepository planoRepo, IPagamentoRepository pagRepo)
        {
            _planoRepo = planoRepo;
            _pagRepo = pagRepo;
        }

        public List<Pagamentos> GetPagamentosAlunos(int alunoId)
        {
            return _pagRepo.Query()
                .Where(p => p.AlunoId == alunoId)
                .OrderByDescending(p => p.DataPagamento)
                .ToList();
        }
        

        public async Task<Pagamentos> RegistraPagamento(PagamentoRegistroDto dto)
        {
            var plano = _planoRepo.GetByAcademia(dto.PlanoId)?.FirstOrDefault();
            if (plano == null)
                throw new Exception("Plano não encontrado");

            var dataPagamento = DateTime.Now;
            var dataVencimento = dataPagamento.AddMonths(plano.DuracaoMeses);

            var pagamento = new Pagamentos
            {
                AlunoId = dto.AlunoId,
                PlanoId = dto.PlanoId,
                Valor = dto.Valor,
                DataPagamento = dataPagamento,
                DataVencimento = dataVencimento,
                Status = "Pg",
                AcademiaId = dto.AcademiaId
            };
            _pagRepo.Add(pagamento);

            return pagamento;
        }






    }
}