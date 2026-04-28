using MA_Sys.API.Dto.Planos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PlanosController : BaseController
    {
        private readonly PlanosService _service;

        public PlanosController(PlanosService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] PlanosFiltroDto filtro)
        {
            var (role, academiaId, userId) = GetUserInfo();
            var planos = _service.Get(role, filtro, academiaId, userId);

            return Ok(planos);
        }

        [HttpGet("totalAlunos")]
        public IActionResult GetTotalAlunos([FromQuery] int planoId)
        {
            var (role, academiaId, userId) = GetUserInfo();
            var totalAlunos = _service.GetTotalAlunos(academiaId, planoId, role, userId);
            return Ok(totalAlunos);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PlanosCreateDto dto)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Add(dto, role, academiaId, userId);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] PlanosUpdateDto dto, int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Update(id, dto, role, academiaId, userId);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.UpdateStatus(id, role, academiaId, userId, ativo);

            return NoContent();
        }
    }
}
