using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Pagamentos;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class PagamentoService
    {
        private readonly IPlanosRepository _planoRepo;
        private readonly IPagamentoRepository _pagRepo;
        private readonly IMatriculaRepository _matriculaRepo;

        public PagamentoService(IPlanosRepository planoRepo, IPagamentoRepository pagRepo, IMatriculaRepository matriculaRepo)
        {
            _planoRepo = planoRepo;
            _pagRepo = pagRepo;
            _matriculaRepo = matriculaRepo;
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

        public void ProcessarWebhook(dynamic payload)
        {
            string externalId = payload?.data?.id?.ToString();
            string status = payload?.data?.status?.ToString();

            if (string.IsNullOrEmpty(externalId))
                throw new Exception("ExternalId inválido");

            var pagamento = _pagRepo.Query()
                .FirstOrDefault(p => p.ExternalId == externalId);

            if (pagamento == null)
                throw new Exception("Pagamento não encontrado");

            if (status == "approved")
            {
                pagamento.Status = "Pago";

                var matricula = _matriculaRepo.Query()
                    .FirstOrDefault(m => m.Id == pagamento.MatriculaId);

                if (matricula != null)
                {
                    matricula.MensalidadePaga = true;
                    matricula.DataPagamento = DateTime.Now;
                }

                _pagRepo.Save();
            }
        }






    }
}