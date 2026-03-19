using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Dto;
using MA_SYS.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login(UserDto dto)
        {
            var usuario = _context.Users
                .FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
                return Unauthorized("Usuário inválido");

            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha))
                return Unauthorized("Senha inválida");

            var token = GerarToken(usuario);

            return Ok(new
            {
                token,
                usuario.AcademiaId
            });
        }
    }
}