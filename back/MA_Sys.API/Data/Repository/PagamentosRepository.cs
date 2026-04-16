using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class PagamentosRepository : BaseRepository<Pagamentos>, IPagamentoRepository
    {
        public PagamentosRepository(AppDbContext context) : base (context){}

        public List<Pagamentos> GetByAcademia(int academiaId)
        {
            return _context.Pagamentos
                .Where(p => p.AcademiaId == academiaId)
                .ToList();
        }
    }
}