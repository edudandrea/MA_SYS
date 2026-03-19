using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto
{
    public class UserDto
    {
        public int Id { get; set; }       
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Role { get; set; }
         public int AcademiaId { get; set; }
        public Academia? Academia { get; set; }
    }
}