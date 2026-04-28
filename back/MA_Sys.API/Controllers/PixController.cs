using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.PixDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PixController : BaseController
    {
        private readonly PixService _pixService;
        private readonly IAcademiaRepository _academiaRepository;

        public PixController(PixService pixService, IAcademiaRepository academiaRepository)
        {
            _pixService = pixService;
            _academiaRepository = academiaRepository;
        }

        [HttpPost("pix")]
        public IActionResult GerarPix([FromBody] PixRequestDto dto)
        {
            var nome = string.IsNullOrWhiteSpace(dto.Nome) ? "ALUNO" : dto.Nome.ToUpperInvariant();
            var cidade = string.IsNullOrWhiteSpace(dto.Cidade)
                ? "CIDADE"
                : dto.Cidade.ToUpperInvariant().Replace(" ", "");

            var academiaId = GetAcademiaId();
            if (!academiaId.HasValue)
            {
                return Unauthorized(new { message = "Usuario sem vinculo com academia." });
            }

            var chavePix = _academiaRepository.Query()
                .Where(a => a.Id == academiaId.Value)
                .Select(a => a.ChavePix)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(chavePix))
            {
                return BadRequest(new { message = "A academia nao possui chave PIX configurada." });
            }

            var payload = _pixService.GerarPixPayload(
                chavePix,
                nome,
                cidade,
                dto.Valor
            );

            return Ok(new PixResponseDto
            {
                Payload = payload,
                Valor = dto.Valor
            });
        }

        [AllowAnonymous]
        [HttpPost("public")]
        public IActionResult GerarPixPublico([FromBody] PixRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Slug))
            {
                return BadRequest(new { message = "Slug da academia nao informado." });
            }

            var academia = _academiaRepository.Query()
                .Where(a => a.Slug == dto.Slug)
                .Select(a => new { a.ChavePix })
                .FirstOrDefault();

            if (academia == null)
            {
                return NotFound(new { message = "Academia nao encontrada." });
            }

            if (string.IsNullOrWhiteSpace(academia.ChavePix))
            {
                return BadRequest(new { message = "A academia nao possui chave PIX configurada." });
            }

            var nome = string.IsNullOrWhiteSpace(dto.Nome) ? "ALUNO" : dto.Nome.ToUpperInvariant();
            var cidade = string.IsNullOrWhiteSpace(dto.Cidade)
                ? "CIDADE"
                : dto.Cidade.ToUpperInvariant().Replace(" ", "");

            var payload = _pixService.GerarPixPayload(
                academia.ChavePix,
                nome,
                cidade,
                dto.Valor
            );

            return Ok(new PixResponseDto
            {
                Payload = payload,
                Valor = dto.Valor
            });
        }

    }
}
