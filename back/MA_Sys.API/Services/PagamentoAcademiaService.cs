using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.PagamentosAcademias;
using MA_Sys.API.Models;
using MA_Sys.API.Security;

namespace MA_Sys.API.Services
{
    public class PagamentoAcademiaService
    {
        private readonly IPagamentoAcademiaRepository _pagamentoAcademiaRepo;
        private readonly IAcademiaRepository _academiaRepo;

        public PagamentoAcademiaService(
            IPagamentoAcademiaRepository pagamentoAcademiaRepo,
            IAcademiaRepository academiaRepo)
        {
            _pagamentoAcademiaRepo = pagamentoAcademiaRepo;
            _academiaRepo = academiaRepo;
        }

        public List<PagamentoAcademiaResponseDto> Listar(string role, int? academiaId, int? userId, int? academiaIdFiltro = null)
        {
            var query = _pagamentoAcademiaRepo.Query();

            if (RoleScope.IsSuperAdmin(role))
            {
                if (academiaIdFiltro.HasValue)
                {
                    query = query.Where(x => x.AcademiaId == academiaIdFiltro.Value);
                }
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(x => academiaIds.Contains(x.AcademiaId));

                if (academiaIdFiltro.HasValue)
                {
                    query = query.Where(x => x.AcademiaId == academiaIdFiltro.Value);
                }
            }
            else
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(x => x.AcademiaId == academiaId.Value);
            }

            return query
                .Join(_academiaRepo.Query(),
                    p => p.AcademiaId,
                    a => a.Id,
                    (p, a) => new PagamentoAcademiaResponseDto
                    {
                        Id = p.Id,
                        AcademiaId = p.AcademiaId,
                        NomeAcademia = a.Nome ?? $"Academia {a.Id}",
                        Valor = p.Valor,
                        DataCriacao = p.DataCriacao,
                        DataVencimento = p.DataVencimento,
                        DataPagamento = p.DataPagamento,
                        Status = p.Status,
                        Descricao = p.Descricao
                    })
                .OrderByDescending(x => x.DataVencimento)
                .ToList();
        }

        public PagamentoAcademia CriarCobranca(PagamentoAcademiaCreateDto dto, string role, int? userId)
        {
            var academiaExiste = _academiaRepo.Query()
                .Any(a => a.Id == dto.AcademiaId &&
                    (RoleScope.IsSuperAdmin(role) ||
                     (RoleScope.IsAdmin(role) && userId.HasValue && a.OwnerUserId == userId.Value)));
            if (!academiaExiste)
                throw new InvalidOperationException("Academia nao encontrada.");

            if (dto.Valor <= 0)
                throw new InvalidOperationException("Valor da cobranca invalido.");

            var cobranca = new PagamentoAcademia
            {
                AcademiaId = dto.AcademiaId,
                Valor = dto.Valor,
                DataCriacao = DateTime.UtcNow,
                DataVencimento = dto.DataVencimento.Date,
                Descricao = dto.Descricao,
                Status = "Pendente"
            };

            _pagamentoAcademiaRepo.Add(cobranca);
            _pagamentoAcademiaRepo.Save();
            return cobranca;
        }

        public void BaixarPagamento(int id, string role, int? userId)
        {
            var query = _pagamentoAcademiaRepo.Query().Where(p => p.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(p => p.Academia.OwnerUserId == userId.Value);
            }
            else
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para baixar cobranca.");
            }

            var cobranca = query.FirstOrDefault();
            if (cobranca == null)
                throw new InvalidOperationException("Cobranca nao encontrada.");

            cobranca.Status = "Pago";
            cobranca.DataPagamento = DateTime.UtcNow;

            _pagamentoAcademiaRepo.Update(cobranca);
            _pagamentoAcademiaRepo.Save();
        }
    }
}
