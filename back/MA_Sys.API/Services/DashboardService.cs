using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Dashboard;
using MA_Sys.API.Dto.DashboardDto;
using MA_Sys.API.Security;

namespace MA_Sys.API.Services
{
    public class DashboardService
    {
        private readonly IProfessorRepository _professorRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IPlanosRepository _planosRepo;
        private readonly IPagamentoRepository _pagamentoRepo;
        private readonly IPagamentoAcademiaRepository _pagamentoAcademiaRepo;
        private readonly IUserRepository _userRepo;

        public DashboardService(
            IAlunoRepository alunoRepo,
            IAcademiaRepository academiaRepo,
            IProfessorRepository professorRepo,
            IPlanosRepository planosRepo,
            IPagamentoRepository pagamentoRepo,
            IPagamentoAcademiaRepository pagamentoAcademiaRepo,
            IUserRepository userRepo)
        {
            _alunoRepo = alunoRepo;
            _academiaRepo = academiaRepo;
            _professorRepo = professorRepo;
            _planosRepo = planosRepo;
            _pagamentoRepo = pagamentoRepo;
            _pagamentoAcademiaRepo = pagamentoAcademiaRepo;
            _userRepo = userRepo;
        }

        public DashboardDto GetDashboard(string role, int? academiaId, int? userId)
        {
            var isSuperAdmin = RoleScope.IsSuperAdmin(role);
            var isAdmin = RoleScope.IsAdmin(role);
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var academiaQuery = _academiaRepo.Query();
            var alunoQuery = _alunoRepo.Query();
            var professoresQuery = _professorRepo.Query();
            var planosQuery = _planosRepo.Query();
            var pagamentosQuery = _pagamentoRepo.Query();
            var pagamentosAcademiaQuery = _pagamentoAcademiaRepo.Query();
            var usuariosQuery = _userRepo.Query();

            if (isSuperAdmin)
            {
            }
            else if (isAdmin)
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                academiaQuery = academiaQuery.Where(a => a.OwnerUserId == userId.Value);
                var academiaIds = academiaQuery.Select(a => a.Id);

                planosQuery = planosQuery.Where(pl => academiaIds.Contains(pl.AcademiaId));
                pagamentosAcademiaQuery = pagamentosAcademiaQuery.Where(pg => academiaIds.Contains(pg.AcademiaId));
                usuariosQuery = usuariosQuery.Where(u => u.UserId == userId.Value || u.CreatedByUserId == userId.Value);

                alunoQuery = alunoQuery.Where(_ => false);
                professoresQuery = professoresQuery.Where(_ => false);
                pagamentosQuery = pagamentosQuery.Where(_ => false);
            }
            else
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar dashboard");

                academiaQuery = academiaQuery.Where(a => a.Id == academiaId.Value);
                alunoQuery = alunoQuery.Where(a => a.AcademiaId == academiaId.Value);
                professoresQuery = professoresQuery.Where(p => p.AcademiaId == academiaId.Value);
                planosQuery = planosQuery.Where(pl => pl.AcademiaId == academiaId.Value);
                pagamentosQuery = pagamentosQuery.Where(pg => pg.AcademiaId == academiaId.Value);
                pagamentosAcademiaQuery = pagamentosAcademiaQuery.Where(pg => pg.AcademiaId == academiaId.Value);
                usuariosQuery = usuariosQuery.Where(u => u.AcademiaId == academiaId.Value);
            }

            var alunosPorMes = alunoQuery
                .GroupBy(a => a.DataCadastro.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Count()
                })
                .OrderBy(g => g.Mes)
                .ToList();

            var planos = planosQuery
                .Select(pl => new PlanoChartsDto
                {
                    Nome = pl.Nome,
                    TotalAlunos = alunoQuery.Count(a => a.PlanoId == pl.Id)
                })
                .ToList();

            var pagamentosRecentes = pagamentosQuery
                .Where(p => p.Status == "Pago" || p.Status == "Pendente" || p.Status == "EmAnalise")
                .ToList()
                .GroupBy(p => p.AlunoId)
                .Select(g => g.OrderByDescending(p => p.DataVencimento).First())
                .ToList();

            var pagamentosAcademiaRecentes = pagamentosAcademiaQuery
                .Where(p => p.Status == "Pago" || p.Status == "Pendente")
                .ToList()
                .GroupBy(p => p.AcademiaId)
                .Select(g => g.OrderByDescending(p => p.DataVencimento).First())
                .ToList();

            var alertasMensalidade = pagamentosRecentes
                .Join(
                    alunoQuery.ToList(),
                    pg => pg.AlunoId,
                    al => al.Id,
                    (pg, al) => new MensalidadeAlertaDto
                    {
                        AlunoId = al.Id,
                        NomeAluno = al.Nome,
                        DataVencimento = pg.DataVencimento,
                        DiasParaVencimento = (pg.DataVencimento.Date - DateTime.UtcNow.Date).Days,
                        Status = pg.Status
                    })
                .Where(x => x.DiasParaVencimento <= 10)
                .OrderBy(x => x.DiasParaVencimento)
                .Take(8)
                .ToList();

            var totalMensalidadesVencendo10Dias = pagamentosRecentes.Count(x =>
            {
                var dias = (x.DataVencimento.Date - DateTime.UtcNow.Date).Days;
                return dias >= 0 && dias <= 10;
            });

            var totalMensalidadesVencidas = pagamentosRecentes.Count(x =>
                (x.DataVencimento.Date - DateTime.UtcNow.Date).Days < 0);

            var academiasPagamentoAlerta = pagamentosAcademiaRecentes
                .Join(
                    academiaQuery.ToList(),
                    pg => pg.AcademiaId,
                    ac => ac.Id,
                    (pg, ac) =>
                    {
                        var vencido = pg.Status == "Pendente" && pg.DataVencimento.Date < DateTime.UtcNow.Date;
                        var pendente = pg.Status == "Pendente";

                        return new AcademiaPagamentoAlertaDto
                        {
                            AcademiaId = ac.Id,
                            NomeAcademia = ac.Nome ?? $"Academia {ac.Id}",
                            TotalPagamentosPendentes = pendente ? 1 : 0,
                            TotalPagamentosVencidos = vencido ? 1 : 0,
                            StatusGeral = vencido || pendente ? "Pendente" : "EmDia"
                        };
                    })
                .OrderByDescending(x => x.TotalPagamentosVencidos)
                .ThenByDescending(x => x.TotalPagamentosPendentes)
                .ToList();

            var receitasMesAcademia = pagamentosQuery
                .Where(p => p.Status == "Pago" && p.DataPagamento >= inicioMes)
                .Sum(p => (decimal?)p.Valor) ?? 0m;

            var receitasMesSistema = pagamentosAcademiaQuery
                .Where(p => p.Status == "Pago" && p.DataPagamento.HasValue && p.DataPagamento.Value >= inicioMes)
                .Sum(p => (decimal?)p.Valor) ?? 0m;

            return new DashboardDto
            {
                TotalAcademias = academiaQuery.Count(),
                TotalAcademiasAtivas = academiaQuery.Count(a => a.Ativo),
                TotalAcademiasInativas = academiaQuery.Count(a => !a.Ativo),
                TotalAlunos = alunoQuery.Count(),
                TotalProfessores = professoresQuery.Count(),
                TotalUsuarios = usuariosQuery.Count(),
                TotalPlanos = planosQuery.Count(),
                ReceitasMes = isAdmin || isSuperAdmin ? receitasMesSistema : receitasMesAcademia,
                DespesasMes = 0m,
                SaldoCaixa = isAdmin || isSuperAdmin ? receitasMesSistema : receitasMesAcademia,
                TotalPagamentosRecebidos = isAdmin || isSuperAdmin
                    ? pagamentosAcademiaQuery.Count(p => p.Status == "Pago")
                    : pagamentosQuery.Count(p => p.Status == "Pago"),
                TotalPagamentosPendentes = isAdmin || isSuperAdmin
                    ? pagamentosAcademiaQuery.Count(p => p.Status == "Pendente")
                    : pagamentosQuery.Count(p => p.Status == "Pendente"),
                Meses = alunosPorMes.Select(x => new DateTime(1, x.Mes, 1).ToString("MMM")).ToList(),
                AlunosPorMes = alunosPorMes.Select(x => x.Total).ToList(),
                Planos = planos,
                TotalMensalidadesVencendo10Dias = isAdmin || isSuperAdmin ? 0 : totalMensalidadesVencendo10Dias,
                TotalMensalidadesVencidas = isAdmin || isSuperAdmin ? 0 : totalMensalidadesVencidas,
                MensalidadesAlerta = isAdmin || isSuperAdmin ? null : alertasMensalidade,
                TotalAcademiasComPendencia = isAdmin || isSuperAdmin
                    ? academiasPagamentoAlerta.Count(x => x.StatusGeral == "Pendente")
                    : 0,
                TotalAcademiasEmDiaPagamento = isAdmin || isSuperAdmin
                    ? academiasPagamentoAlerta.Count(x => x.StatusGeral == "EmDia")
                    : 0,
                AcademiasPagamentoAlerta = isAdmin || isSuperAdmin ? academiasPagamentoAlerta : null
            };
        }
    }
}
