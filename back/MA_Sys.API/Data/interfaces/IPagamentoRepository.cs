using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IPagamentoRepository : IBaseRepository<Pagamentos>
    {
        List<Pagamentos> GetByAcademia(int academiaId);
    }
}