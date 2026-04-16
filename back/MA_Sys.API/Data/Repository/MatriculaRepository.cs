using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class MatriculaRepository : BaseRepository<Matricula>, IMatriculaRepository
    {
        public MatriculaRepository(AppDbContext context) : base(context) { }

         public List<Matricula> GetByAcademia(int academiaId)
        {
            return _context.Matriculas
                .Where(m => m.Plano.AcademiaId == academiaId)
                .ToList();
        }
    }
}