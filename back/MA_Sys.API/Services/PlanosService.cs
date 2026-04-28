using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Planos;
using MA_Sys.API.Security;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class PlanosService
    {
        private readonly IPlanosRepository _repo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IAcademiaRepository _academiaRepo;

        public PlanosService(IPlanosRepository repo, IAlunoRepository alunoRepo, IAcademiaRepository academiaRepo)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
            _academiaRepo = academiaRepo;
        }

        public List<PlanosResponseDto> List(string role, int? academiaId, int? userId)
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

                query = query.Where(a => academiaIds.Contains(a.AcademiaId));
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(a => a.AcademiaId == academiaId);
            }

            return query.Select(a => new PlanosResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Valor = a.Valor,
                DuracaoMeses = a.DuracaoMeses
            }).ToList();
        }

        public List<PlanosResponseDto> Get(string role, PlanosFiltroDto filtro, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();
            var isSuperAdmin = RoleScope.IsSuperAdmin(role);
            var isAdmin = RoleScope.IsAdmin(role);

            if (isSuperAdmin)
            {
            }
            else if (isAdmin)
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(pl => academiaIds.Contains(pl.AcademiaId));
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar planos.");

                query = query.Where(pl => pl.AcademiaId == academiaId);
            }

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Nome))
                    query = query.Where(pl => pl.Nome.Contains(filtro.Nome));

                if (filtro.Ativo.HasValue)
                    query = query.Where(pl => pl.Ativo == filtro.Ativo.Value);
            }

            var academiaIdsAdmin = isAdmin && userId.HasValue
                ? _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList()
                : new List<int>();

            return query.Select(pl => new PlanosResponseDto
            {
                Id = pl.Id,
                Nome = pl.Nome,
                Valor = pl.Valor,
                DuracaoMeses = pl.DuracaoMeses,
                AcademiaId = pl.AcademiaId,
                AcademiaNome = pl.Academia.Nome,
                Ativo = pl.Ativo,
                TotalAlunos = _alunoRepo.Query().Count(a =>
                    a.PlanoId == pl.Id &&
                    (isSuperAdmin || a.AcademiaId == academiaId || (isAdmin && academiaIdsAdmin.Contains(a.AcademiaId))))
            }).ToList();
        }

        public void Add(PlanosCreateDto dto, string role, int? academiaId, int? userId)
        {
            var academiaDestino = RoleScope.IsAcademia(role) ? (academiaId ?? 0) : dto.AcademiaId;

            if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaPertenceAoAdmin = _academiaRepo.Query()
                    .Any(a => a.Id == academiaDestino && a.OwnerUserId == userId.Value);

                if (!academiaPertenceAoAdmin)
                    throw new UnauthorizedAccessException("Admin nao pode cadastrar plano em academia fora do seu escopo.");
            }

            var plano = new Plano
            {
                Nome = dto.Nome,
                Valor = dto.Valor,
                DuracaoMeses = dto.DuracaoMeses,
                AcademiaId = academiaDestino,
                Ativo = true
            };

            _repo.Add(plano);
            _repo.Save();
        }

        public void Update(int id, PlanosUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(a => a.Academia.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var plano = query.FirstOrDefault();

            if (plano == null)
                throw new Exception("Plano nao encontrado");

            plano.Nome = dto.Nome?.Trim();
            plano.Ativo = dto.Ativo;
            plano.Valor = dto.Valor;
            plano.DuracaoMeses = dto.DuracaoMeses;

            _repo.Save();
        }

        public void UpdateStatus(int id, string role, int? academiaId, int? userId, bool ativo)
        {
            var query = _repo.Query().Where(p => p.Id == id);

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
                query = query.Where(p => p.AcademiaId == academiaId);
            }

            var plano = query.FirstOrDefault();

            if (plano == null)
                throw new Exception("Plano nao encontrado");

            plano.Ativo = ativo;

            _repo.Update(plano);
            _repo.Save();
        }

        public PlanosDto GetTotalAlunos(int? academiaId, int? planoId, string? role, int? userId)
        {
            var query = _alunoRepo.Query().Where(a => a.PlanoId == planoId);
            var isSuperAdmin = RoleScope.IsSuperAdmin(role);
            var isAdmin = RoleScope.IsAdmin(role);

            if (!isSuperAdmin && !isAdmin && academiaId.HasValue)
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }
            else if (isAdmin && userId.HasValue)
            {
                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList();

                query = query.Where(a => academiaIds.Contains(a.AcademiaId));
            }

            return new PlanosDto
            {
                TotalAlunos = query.Count()
            };
        }
    }
}
