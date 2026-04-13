using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Planos;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class PlanosService
    {
        private readonly IPlanosRepository _repo;
        private readonly IAlunoRepository _alunoRepo;

        public PlanosService(IPlanosRepository repo, IAlunoRepository alunoRepo)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
        }

        public List<PlanosResponseDto> List(string role, int? academiaId)
        {
            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia");

                query = query.Where(a => a.Id == academiaId);
            }

            return query.Select(a => new PlanosResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Valor = a.Valor,
                DuracaoMeses = a.DuracaoMeses
            }).ToList();
        }

        public List<PlanosResponseDto> Get(string role, PlanosFiltroDto filtro, int? academiaId)
        {
            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia não pode acessar modalidades");

                query = query.Where(pl => pl.AcademiaId == academiaId);
            }


            if (filtro != null && !string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(pl => pl.Nome.Contains(filtro.Nome));

            return query.Select(pl => new PlanosResponseDto
            {
                Id = pl.Id,
                Nome = pl.Nome,
                Valor = pl.Valor,
                DuracaoMeses = pl.DuracaoMeses,
                AcademiaId = pl.AcademiaId,
                AcademiaNome = pl.Academia.Nome,
                Ativo = pl.Ativo,
                TotalAlunos = _alunoRepo.Query().Count(a => a.PlanoId == pl.Id && (role.Trim().ToLower() == "admin" || a.AcademiaId == academiaId))



            }).ToList();
        }

        public void Add(PlanosCreateDto dto, int? academiaId)
        {

            var plano = new Plano
            {
                Nome = dto.Nome,
                Valor = dto.Valor,
                DuracaoMeses = dto.DuracaoMeses,
                AcademiaId = academiaId ?? 0,
                Ativo = true
            };

            _repo.Add(plano);
            _repo.Save();
        }

        public void Update(int id, PlanosUpdateDto dto)
        {
            var plano = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if (plano == null)
                throw new Exception("Plano não encontrado");

            plano.Nome = dto.Nome?.Trim();
            plano.Ativo = dto.Ativo;

            _repo.Save();
        }

        public void UpdateStatus(int id, int? academiaId, bool ativo)
        {
            var plano = _repo.GetById(id, academiaId ?? 0);

            if (plano == null)
                throw new Exception("Plano não encontrado");

            plano.Ativo = ativo;

            _repo.Update(plano);
            _repo.Save();
        }

        public PlanosDto GetTotalAlunos(int? academiaId, int? planoId, string? role)
        {
            var query = _alunoRepo.Query();

            var isAdmin = role?.Trim().ToLower() == "admin";

            // 🔥 filtrar pelo plano
            query = query.Where(a => a.PlanoId == planoId);

            // 🔥 se NÃO for admin → filtra pela academia
            if (!isAdmin && academiaId.HasValue)
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var totalAlunos = query.Count();

            return new PlanosDto
            {
                TotalAlunos = totalAlunos
            };
        }
    }
}