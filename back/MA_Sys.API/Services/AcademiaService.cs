using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.AcademiasDto;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class AcademiaService
    {
        private readonly IAcademiaRepository _repo;

        public AcademiaService(IAcademiaRepository repo)
        {
            _repo = repo;
        }

        public string GerarSlug(string nome)
        {
           var slug = nome.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(";", "")
                .Replace(":", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("&", "")
                .Replace("*", "")
                .Replace("(", "")
                .Replace(")", "");

            return slug;
        }

        public List<AcademiaResponseDto> List(string role, int? academiaId)
        {
            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia");

                query = query.Where(a => a.Id == academiaId);
            }

            return query.Select(a => new AcademiaResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Email = a.Email,
                Telefone = a.Telefone,
                Cidade = a.Cidade,
                RedeSocial = a.RedeSocial,
                Responsavel = a.Responsavel,
                Ativo = a.Ativo,

                totalAlunos = a.Alunos.Count(),
                totalProfessores = a.Professores.Count(),

                Slug = a.Slug

            }).ToList();
        }

        public List<AcademiaResponseDto> Get(string role, AcademiaFiltroDto filtro, int? academiaId)
        {
            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia");

                query = query.Where(a => a.Id == academiaId);
            }

            if (filtro.Id.HasValue)
                query = query.Where(a => a.Id == filtro.Id);

            if (!string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(a => a.Nome.Contains(filtro.Nome));

            return query.Select(a => new AcademiaResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Telefone = a.Telefone,
                Cidade = a.Cidade,
                RedeSocial = a.RedeSocial,
                Responsavel = a.Responsavel,
                Ativo = a.Ativo,

                totalAlunos = a.Alunos.Count(),
                totalProfessores = a.Professores.Count(),

                Slug = a.Slug

            }).ToList();
        }

        public AcademiaDto? GetById(int id, int academiaId)
        {
            return _repo.Query()
                .Where(a => a.Id == id && a.Id == academiaId)
                .Select(a => new AcademiaDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Telefone = a.Telefone,
                    Email = a.Email,

                    Cidade = a.Cidade,
                    Estado = a.Estado,

                    DataCadastro = a.DataCadastro,

                    Slug = a.Slug,

                    Ativo = a.Ativo
                })
                .FirstOrDefault();
        }

        public void Add(AcademiaCreateDto dto)
        {

            var slugBase = GerarSlug(dto.Nome);
            var slug = slugBase;
            int count = 1;

            while (_repo.Query().Any(a => a.Slug == slug))
            {
                slug = $"{slugBase}-{count}";
                count++;
            }
             
            var academia = new Academia
            {
                Nome = dto.Nome,
                Slug = slug,
                Telefone = dto.Telefone,
                Email = dto.Email,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _repo.Add(academia);
            _repo.Save();


        }

        public void Update(int id, AcademiaUpdateDto dto)
        {
            var academia = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if (academia == null)
                throw new Exception("Academia não encontrado");

            academia.Nome = dto.Nome?.Trim();
            academia.Telefone = dto.Telefone;
            academia.Cidade = dto.Cidade;
            academia.Estado = dto.Estado;
            academia.RedeSocial = dto.RedeSocial;
            academia.Email = dto.Email;
            academia.Responsavel = dto.Responsavel;
            

            _repo.Save();
        }

        public void Delete(int id, int? academiaId)
        {
            var academia = _repo.GetById(id, academiaId ?? 0);

            if (academia == null)
                throw new Exception("Academia não encontrado");

            _repo.Delete(academia);
            _repo.Save();
        }

        public void UpdateStatus(int id, int? academiaId, bool ativo)
        {
            var academia = _repo.GetById(id, academiaId ?? 0);

            if (academia == null)
                throw new Exception("Academia não encontrado");

            academia.Ativo = ativo;

            _repo.Update(academia);
            _repo.Save();
        }


    }
}