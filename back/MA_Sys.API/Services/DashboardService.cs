using System.Data;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Dashboard;
using MA_Sys.API.Dto.DashboardDto;

namespace MA_Sys.API.Services
{
    public class DashboardService
    {
        private readonly IProfessorRepository _professorRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IPlanosRepository _planosRepo;
        public DashboardService(IAlunoRepository alunoRepo, 
                                IAcademiaRepository academiaRepo, 
                                IProfessorRepository professorRepo,
                                IPlanosRepository planosRepo)
        {
            _alunoRepo = alunoRepo;
            _academiaRepo = academiaRepo;
            _professorRepo = professorRepo;
            _planosRepo = planosRepo;
        }
        public DashboardDto GetDashboard(string role, int? academiaId)
        {
            var IsAdmin = role?.Trim().ToLower() == "admin";

            var academiaQuery = _academiaRepo.Query();
            var alunoQuery = _alunoRepo.Query();
            var professoresQuery = _professorRepo.Query();
            var planosQuery = _planosRepo.Query();

            if (!IsAdmin)
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia não pode acessar dashboard");

                Console.WriteLine("FILTRANDO POR ACADEMIA: " + academiaId);

                Console.WriteLine(_alunoRepo.Query().Count());
                Console.WriteLine(_alunoRepo.Query().Where(a => a.AcademiaId == academiaId).Count());

                academiaQuery = academiaQuery.Where(a => a.Id == academiaId);
                alunoQuery = alunoQuery.Where(a => a.AcademiaId == academiaId);
                professoresQuery = professoresQuery.Where(p => p.AcademiaId == academiaId);
                planosQuery = planosQuery.Where(pl => pl.AcademiaId == academiaId);
            }

            var alunosPorMes = alunoQuery
                .Where(a => a.DataCadastro != null)
                .GroupBy(a => a.DataCadastro.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Count()
                })
                .OrderBy(g => g.Mes).ToList();

                var planos = planosQuery
                    .Select(pl => new PlanoChartsDto
                    {
                        Nome = pl.Nome,
                        TotalAlunos = alunoQuery.Count(a => a.PlanoId == pl.Id)
                    }).ToList();


            return new DashboardDto
            {
                TotalAcademias = academiaQuery.Count(),
                TotalAlunos = alunoQuery.Count(),
                TotalProfessores = professoresQuery.Count(),

                Meses = alunosPorMes
                        .Select( x => new DateTime(1, x.Mes, 1).ToString("MMM")).ToList(),

                AlunosPorMes = alunosPorMes
                               .Select(x => x.Total).ToList(),

                Planos = planos
            };

        }

        
    }
}