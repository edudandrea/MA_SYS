using MA_Sys.API.Dto.Pagamentos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PagamentosController : BaseController
    {
        private readonly PagamentoService _service;

        public PagamentosController(PagamentoService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegistraPagamento([FromBody] PagamentoRegistroDto dto)
        {
            try
            {
                var pagamento = await _service.RegistraPagamento(dto);

                // 🔥 retorno importante
                return Ok(new
                {
                    pagamentoId = pagamento.Id,
                    status = pagamento.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public IActionResult WebhookPix([FromBody] dynamic payload)
        {
            _service.ProcessarWebhook(payload);
            return Ok();
        }
    }
}