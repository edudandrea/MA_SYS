using MA_SYS.Api.Data;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlunosController : Controller
    {
        private readonly AppDbContext _context;

        public AlunosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        ///  Busca alunos utilizando filtros opcionais. Informe ao menos um filtro para realizar a busca.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetAlunos([FromQuery] int? id,
                                                   [FromQuery] string? nome,
                                                   [FromQuery] string? cpf,
                                                   [FromQuery] int? modalidadeId,
                                                   [FromQuery] string? graduacao)
        {

            if (id == null &&
                string.IsNullOrWhiteSpace(nome) &&
                string.IsNullOrWhiteSpace(cpf) &&
                modalidadeId == null)

            {
                return BadRequest("Informe ao menos um filtro");
            }

            var query = _context.Alunos
                .AsNoTracking()
                .AsQueryable();

            if (id.HasValue)
                query = query.Where(c => c.Id == id.Value);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(c => EF.Functions.Like(c.Nome ?? "", $"%{nome}%"));

            if (!string.IsNullOrWhiteSpace(cpf))
                query = query.Where(c => (c.CPF ?? "").Contains(cpf));

            if (modalidadeId.HasValue)
                query = query.Where(c => c.ModalidadeId == modalidadeId.Value);

            if (!string.IsNullOrWhiteSpace(graduacao))
                query = query.Where(c => (c.Graduacao ?? "").Contains(graduacao));

            var alunos = await query
                .Select(a => new AlunoDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    CPF = a.CPF,
                    Telefone = a.Telefone,
                    CEP = a.CEP,
                    Endereco = a.Endereco,
                    Bairro = a.Bairro,
                    Cidade = a.Cidade,
                    Estado = a.Estado,
                    RedeSocial = a.RedeSocial,
                    Email = a.Email,
                    Sexo = a.Sexo,
                    DataNascimento = a.DataNascimento,
                    DataCadastro = a.DataCadastro,
                    ModalidadeId = a.ModalidadeId,
                    AlunoAtivo = a.AlunoAtivo,     
                    Graduacao = a.Graduacao,               
                })
                .ToListAsync();

            return Ok(alunos);

        }

        /// <summary>
        /// Adiciona um novo aluno
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddAlunos([FromBody] AlunoDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload inválido.");
                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest("Nome é obrigatório.");
                if (string.IsNullOrWhiteSpace(dto.CPF))
                    return BadRequest("CPF é obrigatório.");
                if (dto.ModalidadeId <= 0)
                    return BadRequest("ModalidadeId é obrigatório.");

                var modalidade = await _context.Modalidades
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == dto.ModalidadeId);

                if (modalidade == null)
                    return NotFound("Aluno não encontrado.");

                var alunos = new Aluno
                {
                    Nome = dto.Nome.Trim(),
                    CPF = dto.CPF.Trim(),
                    Telefone = dto.Telefone,
                    Email = dto.Email,
                    CEP = dto.CEP,
                    Endereco = dto.Endereco,
                    Bairro = dto.Bairro,
                    Cidade = dto.Cidade,
                    Estado = dto.Estado,
                    RedeSocial = dto.RedeSocial,
                    Sexo = dto.Sexo,
                    DataCadastro = DateTime.UtcNow,
                    ModalidadeId = dto.ModalidadeId,
                    Graduacao = dto.Graduacao?.Trim(),
                    AlunoAtivo = true
                };

                _context.Alunos.Add(alunos);
                var alunosModalidade = await _context.Modalidades
                        .FirstOrDefaultAsync(m => m.Id == dto.ModalidadeId);

                alunosModalidade?.TotalAlunos += 1;
                await _context.SaveChangesAsync();

                return Ok(new { alunos.Id });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    inner2 = ex.InnerException?.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAlunos(int id, [FromBody] AlunoDto dto)
        {
            try
            {
                var a = await _context.Alunos.FirstOrDefaultAsync(x => x.Id == id);
                if (a == null) return NotFound();

                // validações mínimas (o que é NOT NULL no banco)
                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest("Nome é obrigatório.");
                if (string.IsNullOrWhiteSpace(dto.CPF))
                    return BadRequest("CPF é obrigatório.");

                a.Nome = dto.Nome.Trim();
                a.CPF = dto.CPF;
                a.Telefone = dto.Telefone;
                a.Email = dto.Email;
                a.CEP = dto.CEP;
                a.Endereco = dto.Endereco;
                a.Bairro = dto.Bairro;
                a.Cidade = dto.Cidade;
                a.Estado = dto.Estado;
                a.RedeSocial = dto.RedeSocial;
                a.Sexo = dto.Sexo;
                a.DataNascimento = dto.DataNascimento;
                a.ModalidadeId = dto.ModalidadeId;
                a.Graduacao = dto.Graduacao?.Trim();

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AtualizarStatusAluno(int id, [FromBody] bool ativo)
        {
            var aluno = await _context.Alunos.FindAsync(id);

            if (aluno == null)
                return NotFound();

            var modalidade = await _context.Modalidades.FindAsync(aluno.ModalidadeId);

            if (modalidade == null)
                return BadRequest("Aluno não encontrado");

            
            if (!aluno.AlunoAtivo && ativo)
            {
                modalidade.TotalAlunos += 1;
            }

            
            if (aluno.AlunoAtivo && !ativo && modalidade.TotalAlunos > 0)
            {
                modalidade.TotalAlunos -= 1;
            }

            aluno.AlunoAtivo = ativo;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}