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
    public class ProfessoresController : Controller
    {
        private readonly AppDbContext _context;
        public ProfessoresController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Busca professores utilizando filtros opcionais. Informe ao menos um filtro para realizar a busca.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nome"></param>
        /// <param name="graduacao"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetProfessor([FromQuery] int? id,
                                                   [FromQuery] string? nome,
                                                   [FromQuery] string? graduacao)
        {

            
            var query = _context.Professores
                .AsNoTracking()
                .AsQueryable();

            if (id.HasValue)
                query = query.Where(p => p.Id == id.Value);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => EF.Functions.Like(p.Nome ?? "", $"%{nome}%"));

            if (!string.IsNullOrWhiteSpace(graduacao))
                query = query.Where(p => EF.Functions.Like(p.Graduacao ?? "", $"%{graduacao}%"));


            var professores = await query
                .Select(p => new ProfessorDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Graduacao = p.Graduacao,
                    Telefone = p.Telefone,
                    Email = p.Email,

                })
                .ToListAsync();

            return Ok(professores);

        }

        /// <summary>
        /// Adiciona um novo professor. Os campos "Nome" e "Graduação" são obrigatórios.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddProfessor([FromBody] ProfessorDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload inválido.");
                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest("Nome é obrigatório.");
                

                // Verifica se a academia existe e está ativa
                var academia = await _context.Academias
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == dto.AcademiaId);

                if (academia == null)
                    return NotFound("Academia não encontrada.");

                if (!academia.Ativo)
                    return BadRequest("Não é possível cadastrar professor em uma academia desativada.");


                var professores = new Professor
                {
                    Nome = dto.Nome.Trim(),
                    Graduacao = dto.Graduacao?.Trim(),
                    Email = dto.Email,
                    Telefone = dto.Telefone,
                    AcademiaId = dto.AcademiaId,
                    Ativo = true
                };

                _context.Professores.Add(professores);
                var profModalidade = await _context.Modalidades
                        .FirstOrDefaultAsync(m => m.Id == dto.ModalidadeId);

                profModalidade?.TotalProf += 1;
                await _context.SaveChangesAsync();

                return Ok(new { professores.Id });
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
        public async Task<IActionResult> UpdateProfessor(int id, [FromBody] ProfessorDto dto)
        {
            try
            {
                var p = await _context.Professores.FirstOrDefaultAsync(x => x.Id == id);
                if (p == null) return NotFound();

                // validações mínimas (o que é NOT NULL no banco)
                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest("Nome é obrigatório.");
                if (string.IsNullOrWhiteSpace(dto.Graduacao))
                    return BadRequest("Graduação é obrigatória.");

                p.Id = id;
                p.Nome = dto.Nome.Trim();
                p.AcademiaId = dto.AcademiaId;
                p.Graduacao = dto.Graduacao.Trim();
                p.Email = dto.Email;
                p.Telefone = dto.Telefone;
                
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

        /// <summary>
        /// Remove um professor existente. Informe o ID do professor a ser removido. Retorna mensagem de sucesso ou erro caso o professor não seja encontrado ou ocorra algum problema durante a remoção.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteProfessor(int id)
        {
            try
            {
                var professor = await _context.Professores.FindAsync(id);

                if (professor == null)
                    return NotFound($"Professor com ID {id} não encontrado.");

                _context.Professores.Remove(professor);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Professor removido com sucesso." });
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

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var professor = await _context.Professores.FindAsync(id);

            if (professor == null)
                return NotFound();

            var modalidade = await _context.Modalidades.FindAsync(professor.ModalidadeId);

            if (modalidade == null)
                return BadRequest("Aluno não encontrado");

            
            if (!professor.Ativo && ativo)
            {
                modalidade.TotalProf += 1;
            }

            
            if (professor.Ativo && !ativo && modalidade.TotalProf > 0)
            {
                modalidade.TotalProf -= 1;
            }


            professor.Ativo = ativo;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}