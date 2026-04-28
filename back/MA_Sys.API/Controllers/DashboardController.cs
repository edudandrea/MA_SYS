using MA_Sys.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var (role, academiaId, userId) = GetUserInfo();
            var dashboard = _service.GetDashboard(role, academiaId, userId);
            return Ok(dashboard);
        }
    }
}
