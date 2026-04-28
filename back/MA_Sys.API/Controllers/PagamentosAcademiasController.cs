using MA_Sys.API.Dto.PagamentosAcademias;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PagamentosAcademiasController : BaseController
    {
        private readonly PagamentoAcademiaService _service;

        public PagamentosAcademiasController(PagamentoAcademiaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? academiaId)
        {
            var (role, academiaIdUsuario, userId) = GetUserInfo();
            return Ok(_service.Listar(role, academiaIdUsuario, userId, academiaId));
        }

        [HttpPost]
        public IActionResult CriarCobranca([FromBody] PagamentoAcademiaCreateDto dto)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            var cobranca = _service.CriarCobranca(dto, GetUserRole(), GetUserId());
            return Ok(cobranca);
        }

        [HttpPatch("{id}/baixar")]
        public IActionResult Baixar(int id)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            _service.BaixarPagamento(id, GetUserRole(), GetUserId());
            return NoContent();
        }
    }
}
