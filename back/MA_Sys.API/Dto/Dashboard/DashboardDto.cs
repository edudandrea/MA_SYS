using MA_Sys.API.Dto.Dashboard;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.DashboardDto
{
    public class DashboardDto
    {
        public int TotalAcademias { get; set; }
        public int TotalAlunos { get; set; }
        public int TotalProfessores { get; set; }
        public int AcademiasEmDia { get; set; }
        public int AcademiasAtraso { get; set; }
        public int TotalPlanos { get; set; }
        public List<string>? Meses { get; set; }
        public List<Academia>? AcademiasAtrasadas { get; set; }
        public List<Academia>? AlunosPorAcademia { get; set; }
        public List<Plano>? PlanosPorAcademia { get; set; }
        public List<int>? AlunosPorMes { get; set; }
        public List<PlanoChartsDto>? Planos { get; set; }
    }
}