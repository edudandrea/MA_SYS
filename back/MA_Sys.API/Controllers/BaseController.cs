using System.Security.Claims;
using MA_Sys.API.Data.Repository.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [Route("[controller]")]
    public class BaseController : Controller
    {
        protected string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        protected int? GetAcademiaId()
        {
            var claim = User.FindFirst("AcademiaId");
            if (claim == null || string.IsNullOrEmpty(claim.Value))

                return null;
            if (int.TryParse(claim.Value, out int academiaId))

                return academiaId;

            return null;
        }

        protected (string role, int? academiaId) GetUserInfo()
        {
            return (GetUserRole(), GetAcademiaId());
        }

        protected int ObterAcademiaIdPeloSlug(string slug)
        {
            var academiaRepo = HttpContext.RequestServices
                .GetService(typeof(IAcademiaRepository)) as IAcademiaRepository;

            var academia = academiaRepo.Query()
                .FirstOrDefault(a => a.Slug == slug);

            if (academia == null)
                throw new Exception("Academia não encontrada");

            return academia.Id;
        }
    }
}