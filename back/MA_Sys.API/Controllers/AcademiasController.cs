using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
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
    public class AcademiasController : Controller
    {
        private readonly AppDbContext _context;

        public AcademiasController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Busca academias utilizando filtros opcionais. Informe ao menos um filtro para realizar a busca.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomeAcademia"></param>
        /// <param name="cidade"></param>
        /// <param name="ativo"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetAcademias([FromQuery] int? id,
                                                      [FromQuery] string? nome,
                                                      [FromQuery] string? cidade, [FromQuery] bool? ativo)
        {

            var query = _context.Academias
                 .AsNoTracking()
                 .AsQueryable();

            if (id.HasValue)
                query = query.Where(c => c.Id == id.Value);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(c => EF.Functions.Like(c.Nome ?? "", $"%{nome}%"));
            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(c => EF.Functions.Like(c.Cidade ?? "", $"%{cidade}%"));
            if (ativo.HasValue)
                query = query.Where(c => c.Ativo == ativo.Value);



            var academia = await query
                .Select(a => new AcademiaDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Cidade = a.Cidade,
                    Email = a.Email,
                    RedeSocial = a.RedeSocial,
                    Telefone = a.Telefone,
                    Responsavel = a.Responsavel,
                    DataCadastro = a.DataCadastro,
                    Ativo = a.Ativo
                })
                .ToListAsync();

            return Ok(academia);
        }

        /// <summary>
        /// Adiciona uma nova academia. Campos obrigatórios: NomeAcademia, Cidade.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddAcademia([FromBody] AcademiaDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload inválido.");
                if (string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("Nome da academia é obrigatório.");
                if (string.IsNullOrWhiteSpace(dto.Cidade)) return BadRequest("Cidade é obrigatória.");


                var academia = new Academia
                {
                    Nome = dto.Nome?.Trim(),
                    Cidade = dto.Cidade?.Trim(),
                    Email = dto.Email?.Trim(),
                    RedeSocial = dto.RedeSocial,
                    Responsavel = dto.Responsavel,
                    Telefone = dto.Telefone,
                    DataCadastro = DateTime.UtcNow,
                    Ativo = true,

                };

                Console.WriteLine(JsonSerializer.Serialize(dto));

                _context.Academias.Add(academia);
                await _context.SaveChangesAsync();

                return Ok(new { academia.Id });
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
        /// Atualiza uma academia existente. Campos obrigatórios: NomeAcademia, Cidade, Email.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPut("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAcademia(int id, [FromBody] AcademiaDto dto)
        {
            try
            {
                var a = await _context.Academias.FirstOrDefaultAsync(x => x.Id == id);
                if (a == null) return NotFound();

                // validações mínimas (o que é NOT NULL no banco)
                if (string.IsNullOrWhiteSpace(dto.Nome))
                    return BadRequest("Nome da academia é obrigatório.");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("Email é obrigatório.");

                a.Nome = dto.Nome.Trim();
                a.Cidade = dto.Cidade?.Trim();
                a.Email = dto.Email?.Trim();
                a.RedeSocial = dto.RedeSocial;
                a.Telefone = dto.Telefone;
                a.Responsavel = dto.Responsavel;

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
            var academia = await _context.Academias.FindAsync(id);

            if (academia == null)
                return NotFound();

            academia.Ativo = ativo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAcademia(int id)
        {
            try
            {
                var academia = await _context.Academias.FindAsync(id);

                if (academia == null)
                    return NotFound($"Academia com ID {id} não encontrada.");

                _context.Academias.Remove(academia);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Academia removida com sucesso." });
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

