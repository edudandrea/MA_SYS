using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IFormaPagamentoRepository : IBaseRepository<FormaPagamento>
    {
        List<FormaPagamento> GetByAcademia(int academiaId);
    }
}