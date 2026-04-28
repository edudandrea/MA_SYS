using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.FormaPagamentos;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class FormaPagamentoService
    {
        private readonly IFormaPagamentoRepository _repo;
        private readonly IAcademiaRepository _academiaRepo;

        public FormaPagamentoService(IFormaPagamentoRepository repo, IAcademiaRepository academiaRepo)
        {
            _repo = repo;
            _academiaRepo = academiaRepo;
        }

        public List<FormaPagamentoResponseDto> List(int academiaId)
        {
            return _repo.Query()
                .Where(m => m.AcademiaId == academiaId)
                .Select(a => new FormaPagamentoResponseDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Ativo = a.Ativo,
                    Taxa = a.Taxa,
                    Parcelas = a.Parcelas,
                    Dias = a.Dias
                }).ToList();
        }

        public List<FormaPagamentoResponseDto> Get(string role, FormaPagamentoFiltroDto filtro, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(m => academiaIds.Contains(m.AcademiaId));
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar formas de pagamento.");

                query = query.Where(m => m.AcademiaId == academiaId);
            }

            if (filtro != null && !string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(m => m.Nome.Contains(filtro.Nome));

            return query.Select(m => new FormaPagamentoResponseDto
            {
                Id = m.Id,
                Nome = m.Nome,
                Ativo = m.Ativo,
                Taxa = m.Taxa,
                Parcelas = m.Parcelas,
                Dias = m.Dias
            }).ToList();
        }

        public void Add(FormaPagamentoCreateDto dto, int? academiaId, string role, int? userId)
        {
            var academiaDestino = RoleScope.IsAcademia(role) ? academiaId ?? 0 : dto.AcademiaId;

            if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role) && academiaId == null)
                throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode criar formas de pagamento.");

            if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaPertenceAoAdmin = _academiaRepo.Query()
                    .Any(a => a.Id == academiaDestino && a.OwnerUserId == userId.Value);

                if (!academiaPertenceAoAdmin)
                    throw new UnauthorizedAccessException("Admin nao pode cadastrar formas de pagamento fora do seu escopo.");
            }

            var formaPagamento = new FormaPagamento
            {
                Nome = dto.Nome,
                AcademiaId = academiaDestino,
                Ativo = true,
                Taxa = dto.Taxa,
                Parcelas = dto.Parcelas,
                Dias = dto.Dias
            };

            _repo.Add(formaPagamento);
            _repo.Save();
        }

        public void Update(int id, FormaPagamentoUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList();

                query = query.Where(a => academiaIds.Contains(a.AcademiaId));
            }
            else
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var formaPagamento = query.FirstOrDefault();

            if (formaPagamento == null)
                throw new Exception("Forma de pagamento nao encontrada");

            formaPagamento.Nome = dto.Nome;
            formaPagamento.Ativo = dto.Ativo;
            formaPagamento.Taxa = dto.Taxa;
            formaPagamento.Parcelas = dto.Parcelas;
            formaPagamento.Dias = dto.Dias;

            _repo.Save();
        }

        public void UpdateStatus(int id, string role, int? academiaId, int? userId, bool ativo)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList();

                query = query.Where(a => academiaIds.Contains(a.AcademiaId));
            }
            else
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var formaPagamento = query.FirstOrDefault();

            if (formaPagamento == null)
                throw new Exception("Forma de pagamento nao encontrada");

            formaPagamento.Ativo = ativo;

            _repo.Update(formaPagamento);
            _repo.Save();
        }
    }
}
