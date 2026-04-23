using MA_Sys.API.Controllers;
using MA_Sys.API.Dto.AcademiasDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AcademiasController : BaseController
    {
        private readonly AcademiaService _service;

        public AcademiasController(AcademiaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult List()
        {
            var (role, academiaId) = GetUserInfo();
            var academias = _service.List(role, academiaId);
            return Ok(academias);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get([FromBody] AcademiaFiltroDto filtro)
        {
            var (role, academiaId) = GetUserInfo();

            var academia = _service.Get(role, filtro, academiaId);

            return Ok(academia);
        }


        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add([FromBody] AcademiaCreateDto dto)
        {
            _service.Add(dto);

            return Ok(new
            {
                sucesso = true,
                mensagem = "Academia cadastrada com sucesso"
            }

            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] AcademiaUpdateDto dto, int id)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Update(id, dto);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var academiaId = GetAcademiaId();
            _service.UpdateStatus(id, academiaId, ativo);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var (role, academiaId) = GetUserInfo();
            _service.Delete(id, academiaId);

            return NoContent();
        }

    }
}

