using MA_Sys.API.Dto.Dashboard;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.DashboardDto
{
    public class DashboardDto
    {
        public int TotalAcademias { get; set; }
        public int TotalAcademiasAtivas { get; set; }
        public int TotalAcademiasInativas { get; set; }
        public int TotalAlunos { get; set; }
        public int TotalProfessores { get; set; }
        public int TotalUsuarios { get; set; }
        public int AcademiasEmDia { get; set; }
        public int AcademiasAtraso { get; set; }
        public int TotalPlanos { get; set; }
        public decimal ReceitasMes { get; set; }
        public decimal DespesasMes { get; set; }
        public decimal SaldoCaixa { get; set; }
        public int TotalPagamentosRecebidos { get; set; }
        public int TotalPagamentosPendentes { get; set; }
        public List<string>? Meses { get; set; }
        public List<Academia>? AcademiasAtrasadas { get; set; }
        public List<Academia>? AlunosPorAcademia { get; set; }
        public List<Plano>? PlanosPorAcademia { get; set; }
        public List<int>? AlunosPorMes { get; set; }
        public List<PlanoChartsDto>? Planos { get; set; }
        public int TotalMensalidadesVencendo10Dias { get; set; }
        public int TotalMensalidadesVencidas { get; set; }
        public int NovosAlunosMes { get; set; }
        public int AlunosInativos { get; set; }
        public decimal TaxaEvasao { get; set; }
        public ModalidadeForteDashboardDto? ModalidadeMaisForte { get; set; }
        public List<ProfessorAlunosDashboardDto>? ProfessoresComMaisAlunos { get; set; }
        public List<MensalidadeAlertaDto>? MensalidadesAlerta { get; set; }
        public int TotalAcademiasComPendencia { get; set; }
        public int TotalAcademiasEmDiaPagamento { get; set; }
        public List<AcademiaPagamentoAlertaDto>? AcademiasPagamentoAlerta { get; set; }
    }

    public class ModalidadeForteDashboardDto
    {
        public int ModalidadeId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int TotalAlunos { get; set; }
    }

    public class ProfessorAlunosDashboardDto
    {
        public int ProfessorId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int TotalAlunos { get; set; }
    }
}
