using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IPlanosRepository : IBaseRepository<Plano>
    {
        List<Plano> GetByAcademia(int academiaId);
    }
}