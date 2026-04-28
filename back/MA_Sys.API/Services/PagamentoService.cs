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
        private readonly IFormaPagamentoRepository _formaPagamentoRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly MercadoPagoGatewayService _mercadoPagoGateway;

        public PagamentoService(
            IPlanosRepository planoRepo,
            IPagamentoRepository pagRepo,
            IMatriculaRepository matriculaRepo,
            IFormaPagamentoRepository formaPagamentoRepo,
            IAcademiaRepository academiaRepo,
            MercadoPagoGatewayService mercadoPagoGateway)
        {
            _planoRepo = planoRepo;
            _pagRepo = pagRepo;
            _matriculaRepo = matriculaRepo;
            _formaPagamentoRepo = formaPagamentoRepo;
            _academiaRepo = academiaRepo;
            _mercadoPagoGateway = mercadoPagoGateway;
        }

        public List<Pagamentos> GetPagamentosAlunos(int alunoId)
        {
            return _pagRepo.Query()
                .Where(p => p.AlunoId == alunoId)
                .OrderByDescending(p => p.DataPagamento)
                .ToList();
        }

        public Task<Pagamentos> RegistraPagamento(PagamentoRegistroDto dto)
        {
            var plano = _planoRepo.GetByAcademia(dto.PlanoId)?.FirstOrDefault();
            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado.");
            }

            var dataPagamento = DateTime.UtcNow;
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
            return Task.FromResult(pagamento);
        }

        public async Task<Pagamentos> ProcessarPagamentoCartaoPublico(PagamentoCartaoPublicoDto dto, int academiaId)
        {
            if (dto.AlunoId <= 0 || dto.MatriculaId <= 0 || dto.PlanoId <= 0)
            {
                throw new InvalidOperationException("Dados da matricula invalidos para processar o pagamento.");
            }

            if (dto.FormaPagamentoId <= 0)
            {
                throw new InvalidOperationException("Forma de pagamento invalida.");
            }

            if (string.IsNullOrWhiteSpace(dto.CardToken))
            {
                throw new InvalidOperationException("Token de cartao nao informado.");
            }

            var matricula = _matriculaRepo.Query()
                .FirstOrDefault(m =>
                    m.Id == dto.MatriculaId &&
                    m.AlunoId == dto.AlunoId &&
                    m.PlanoId == dto.PlanoId &&
                    m.AcademiaId == academiaId);

            if (matricula == null)
            {
                throw new InvalidOperationException("Matricula nao encontrada para este pagamento.");
            }

            var plano = _planoRepo.Query()
                .FirstOrDefault(p => p.Id == dto.PlanoId && p.AcademiaId == academiaId);

            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado para este aluno.");
            }

            var formaPagamento = _formaPagamentoRepo.Query()
                .FirstOrDefault(f =>
                    f.Id == dto.FormaPagamentoId &&
                    f.AcademiaId == academiaId &&
                    f.Ativo);

            if (formaPagamento == null)
            {
                throw new InvalidOperationException("Forma de pagamento nao encontrada ou indisponivel.");
            }

            if (!formaPagamento.Nome.Contains("credito", StringComparison.OrdinalIgnoreCase) &&
                !formaPagamento.Nome.Contains("crédito", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A forma de pagamento selecionada nao e valida para cartao de credito.");
            }

            var valorEsperado = plano.Valor;
            if (dto.Valor <= 0 || decimal.Round(dto.Valor, 2) != decimal.Round(valorEsperado, 2))
            {
                throw new InvalidOperationException("O valor informado nao corresponde ao plano contratado.");
            }

            var academia = _academiaRepo.Query()
                .FirstOrDefault(a => a.Id == academiaId);

            if (academia == null)
            {
                throw new InvalidOperationException("Academia nao encontrada para processar o pagamento.");
            }

            if (string.IsNullOrWhiteSpace(academia.MercadoPagoAccessToken))
            {
                throw new InvalidOperationException("A academia ainda nao configurou o token de recebimento do cartao.");
            }

            var descricao = $"Plano {plano.Nome} - Aluno {dto.AlunoId}";
            var gatewayResult = await _mercadoPagoGateway.ProcessarPagamentoCartaoAsync(
                dto,
                descricao,
                academia.MercadoPagoAccessToken);

            var statusPagamento = gatewayResult.Status.ToLowerInvariant() switch
            {
                "approved" => "Pago",
                "pending" => "Pendente",
                "in_process" => "EmAnalise",
                _ => "Recusado"
            };

            var agora = DateTime.UtcNow;
            var pagamento = new Pagamentos
            {
                AcademiaId = academiaId,
                AlunoId = dto.AlunoId,
                MatriculaId = dto.MatriculaId,
                PlanoId = dto.PlanoId,
                FormaPagamentoId = dto.FormaPagamentoId,
                Valor = dto.Valor,
                DataPagamento = agora,
                DataVencimento = agora.AddMonths(plano.DuracaoMeses),
                Status = statusPagamento,
                ExternalId = gatewayResult.ExternalId
            };

            _pagRepo.Add(pagamento);

            if (statusPagamento == "Pago")
            {
                matricula.MensalidadePaga = true;
                matricula.DataPagamento = agora;
                matricula.FormaPagamentoId = dto.FormaPagamentoId;
            }

            _pagRepo.Save();

            return pagamento;
        }

        public void ProcessarWebhook(dynamic payload)
        {
            string externalId = payload?.data?.id?.ToString();
            string status = payload?.data?.status?.ToString();

            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new InvalidOperationException("ExternalId invalido.");
            }

            var pagamento = _pagRepo.Query()
                .FirstOrDefault(p => p.ExternalId == externalId);

            if (pagamento == null)
            {
                throw new InvalidOperationException("Pagamento nao encontrado.");
            }

            if (status == "approved")
            {
                pagamento.Status = "Pago";

                var matricula = _matriculaRepo.Query()
                    .FirstOrDefault(m => m.Id == pagamento.MatriculaId);

                if (matricula != null)
                {
                    matricula.MensalidadePaga = true;
                    matricula.DataPagamento = DateTime.UtcNow;
                }

                _pagRepo.Save();
            }
        }

        public Pagamentos? ObterPagamentoPorId(int pagamentoId, int academiaId)
        {
            return _pagRepo.Query()
                .FirstOrDefault(p => p.Id == pagamentoId && p.AcademiaId == academiaId);
        }

    }
}
