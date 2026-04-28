using MA_Sys.API.Dto.FluxoCaixa;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FluxoCaixaController : BaseController
    {
        private readonly FluxoCaixaService _service;

        public FluxoCaixaController(FluxoCaixaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] FluxoCaixaFiltroDto filtro)
        {
            var (role, academiaId, userId) = GetUserInfo();
            return Ok(_service.Listar(role, academiaId, userId, filtro));
        }

        [HttpPost]
        public IActionResult Lancar([FromBody] FluxoCaixaCreateDto dto)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Lancar(role, academiaId, userId, dto);
            return NoContent();
        }
    }
}
