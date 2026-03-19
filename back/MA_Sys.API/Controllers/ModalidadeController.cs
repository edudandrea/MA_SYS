using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Data;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModalidadeController : Controller
    {
        private readonly AppDbContext _context;
        public ModalidadeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Busca modalidades utilizando filtros opcionais. Informe ao menos um filtro para realizar a busca.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomeModalidade"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetModalidade([FromQuery] int? id,
                                                   [FromQuery] string? nomeModalidade,
                                                   [FromQuery] bool? ativo)
        {

            var query = _context.Modalidades
                .AsNoTracking()
                .AsQueryable();

            if (id.HasValue)
                query = query.Where(m => m.Id == id.Value);

            if (!string.IsNullOrWhiteSpace(nomeModalidade))
                query = query.Where(m => EF.Functions.Like(m.NomeModalidade ?? "", $"%{nomeModalidade}%"));
            if (ativo.HasValue)
                query = query.Where(m => m.Ativo == ativo.Value);



            var modalidades = await query
                .Select(m => new ModalidadeDto
                {
                    Id = m.Id,
                    NomeModalidade = m.NomeModalidade,
                    Ativo = m.Ativo,
                    TotalAlunos = _context.Alunos
                                  .Count(a => a.ModalidadeId == m.Id && a.AlunoAtivo)
                })
                .ToListAsync();

            return Ok(modalidades);
        }

        /// <summary>
        /// Adiciona uma nova modalidade. Campos obrigatórios: NomeModalidade.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddModalidade([FromBody] ModalidadeDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload inválido.");
                if (string.IsNullOrWhiteSpace(dto.NomeModalidade)) return BadRequest("Nome da modalidade é obrigatório.");


                var modalidades = new Modalidade
                {
                    NomeModalidade = dto.NomeModalidade.Trim(),
                    Ativo = true
                };

                _context.Modalidades.Add(modalidades);
                await _context.SaveChangesAsync();

                return Ok(new { modalidades.Id });
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

        /// <summary>
        /// Atualiza uma modalidade existente. Campos obrigatórios: NomeModalidade.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPut("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Updatemodalidade(int id, [FromBody] ModalidadeDto dto)
        {
            try
            {
                var m = await _context.Modalidades.FirstOrDefaultAsync(x => x.Id == id);
                if (m == null) return NotFound();

                // validações mínimas (o que é NOT NULL no banco)
                if (string.IsNullOrWhiteSpace(dto.NomeModalidade))
                    return BadRequest("Nome da modalidade é obrigatório.");

                m.Id = id;
                m.NomeModalidade = dto.NomeModalidade.Trim();
                m.Ativo = dto.Ativo;


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
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var modalidade = await _context.Modalidades.FindAsync(id);

            if (modalidade == null)
                return NotFound();

            modalidade.Ativo = ativo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteModalidade(int id)
        {
            try
            {
                var modalidade = await _context.Modalidades.FindAsync(id);

                if (modalidade == null)
                    return NotFound($"Modalidade com ID {id} não encontrada.");

                _context.Modalidades.Remove(modalidade);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Modalidade removida com sucesso." });
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
    }
}
