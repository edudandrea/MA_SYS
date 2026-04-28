using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Models;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Data.Repository
{
    public class FinanceiroRepository : BaseRepository<Financeiro>, IFinanceiroRepository
    {
        public FinanceiroRepository(AppDbContext context) : base(context)
        {
        }
    }
}
