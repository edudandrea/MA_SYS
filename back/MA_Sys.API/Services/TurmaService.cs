using MA_Sys.API.Dto.Turmas;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class TurmaService
    {
        private readonly AppDbContext _context;

        public TurmaService(AppDbContext context)
        {
            _context = context;
        }

        public List<TurmaResponseDto> List(int academiaId)
        {
            var hoje = DateTime.UtcNow.Date;
            // Execute database query first
            var turmas = _context.Set<Turma>()
                .AsNoTracking()
                .Where(t => t.AcademiaId == academiaId)
                .Include(t => t.Professor)
                .Include(t => t.Alunos)
                .ThenInclude(ta => ta.Aluno)
                .OrderBy(t => t.Nome)
                .ToList();
            var turmaIds = turmas.Select(t => t.Id).ToList();
            var checkIns = _context.CheckInsAulas
                .AsNoTracking()
                .Where(c => c.AcademiaId == academiaId && turmaIds.Contains(c.TurmaId) && c.DataCheckIn.Date >= hoje)
                .ToList();

            // Then do client-side processing
            return turmas
                .Select(t => new TurmaResponseDto
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Descricao = t.Descricao,
                    ProfessorId = t.ProfessorId,
                    ProfessorNome = t.Professor?.Nome,
                    DiasSemana = string.IsNullOrWhiteSpace(t.DiasSemana)
                        ? new List<string>()
                        : t.DiasSemana.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList(),
                    Ativo = t.Ativo,
                    Alunos = t.Alunos
                        .OrderBy(a => a.Aluno!.Nome)
                        .Select(a =>
                        {
                            var checkInsAluno = checkIns
                                .Where(c => c.TurmaId == t.Id && c.AlunoId == a.AlunoId)
                                .OrderBy(c => c.DataCheckIn)
                                .ToList();
                            var proximoCheckIn = checkInsAluno.FirstOrDefault();

                            return new TurmaAlunoDto
                            {
                                AlunoId = a.AlunoId,
                                Nome = a.Aluno!.Nome ?? string.Empty,
                                CheckInProximaAula = proximoCheckIn != null,
                                DataCheckInProximaAula = proximoCheckIn?.DataCheckIn,
                                DiaSemanaCheckIn = proximoCheckIn != null ? ObterNomeDiaSemana(proximoCheckIn.DataCheckIn) : null,
                                CheckInsAulas = checkInsAluno
                                    .Select(c => new TurmaAlunoCheckInDto
                                    {
                                        CheckInId = c.Id,
                                        DataAula = c.DataCheckIn,
                                        DiaSemana = ObterNomeDiaSemana(c.DataCheckIn)
                                    })
                                    .ToList()
                            };
                        })
                        .ToList()
                })
                .ToList();
        }

        public TurmaResponseDto Add(TurmaCreateUpdateDto dto, int academiaId)
        {
            var turma = new Turma
            {
                AcademiaId = academiaId,
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao?.Trim(),
                ProfessorId = ValidarProfessor(dto.ProfessorId, academiaId),
                DiasSemana = string.Join(",", dto.DiasSemana.Distinct(StringComparer.OrdinalIgnoreCase)),
                Ativo = dto.Ativo
            };

            _context.Set<Turma>().Add(turma);
            _context.SaveChanges();

            AdicionarAlunos(turma.Id, dto.AlunoIds);
            _context.SaveChanges();
            return GetById(turma.Id, academiaId)!;
        }

        public TurmaResponseDto Update(int id, TurmaCreateUpdateDto dto, int academiaId)
        {
            var turma = _context.Set<Turma>()
                .Include(t => t.Alunos)
                .FirstOrDefault(t => t.Id == id && t.AcademiaId == academiaId);

            if (turma == null)
            {
                throw new InvalidOperationException("Turma nao encontrada.");
            }

            turma.Nome = dto.Nome.Trim();
            turma.Descricao = dto.Descricao?.Trim();
            turma.ProfessorId = ValidarProfessor(dto.ProfessorId, academiaId);
            turma.DiasSemana = string.Join(",", dto.DiasSemana.Distinct(StringComparer.OrdinalIgnoreCase));
            turma.Ativo = dto.Ativo;

            AdicionarAlunos(turma.Id, dto.AlunoIds);
            _context.SaveChanges();
            return GetById(turma.Id, academiaId)!;
        }

        public TurmaResponseDto AdicionarAluno(int turmaId, int alunoId, int academiaId)
        {
            ValidarTurma(turmaId, academiaId);
            ValidarAluno(alunoId, academiaId);
            AdicionarAlunos(turmaId, new[] { alunoId });
            _context.SaveChanges();
            return GetById(turmaId, academiaId)!;
        }

        public TurmaResponseDto RemoverAluno(int turmaId, int alunoId, int academiaId)
        {
            ValidarTurma(turmaId, academiaId);

            var vinculo = _context.Set<TurmaAluno>()
                .FirstOrDefault(ta => ta.TurmaId == turmaId && ta.AlunoId == alunoId);

            if (vinculo == null)
            {
                throw new InvalidOperationException("Aluno nao esta vinculado a esta turma.");
            }

            var checkIns = _context.CheckInsAulas
                .Where(c => c.TurmaId == turmaId && c.AlunoId == alunoId && c.AcademiaId == academiaId);

            _context.CheckInsAulas.RemoveRange(checkIns);
            _context.Set<TurmaAluno>().Remove(vinculo);
            _context.SaveChanges();
            return GetById(turmaId, academiaId)!;
        }

        public TurmaResponseDto? GetById(int id, int academiaId)
        {
            return List(academiaId).FirstOrDefault(t => t.Id == id);
        }

        public void Delete(int id, int academiaId)
        {
            var turma = _context.Set<Turma>()
                .Include(t => t.Alunos)
                .FirstOrDefault(t => t.Id == id && t.AcademiaId == academiaId);

            if (turma == null)
            {
                throw new InvalidOperationException("Turma nao encontrada.");
            }

            if (turma.Alunos.Count > 0)
            {
                _context.Set<TurmaAluno>().RemoveRange(turma.Alunos);
            }

            _context.Set<Turma>().Remove(turma);
            _context.SaveChanges();
        }

        private void AdicionarAlunos(int turmaId, IEnumerable<int> alunoIds)
        {
            var ids = alunoIds.Distinct().ToList();
            var atuais = _context.Set<TurmaAluno>().Where(ta => ta.TurmaId == turmaId).ToList();

            var existentes = atuais.Select(a => a.AlunoId).ToHashSet();
            var novos = ids.Where(id => !existentes.Contains(id))
                .Select(id => new TurmaAluno { TurmaId = turmaId, AlunoId = id });

            _context.Set<TurmaAluno>().AddRange(novos);
        }

        private void ValidarTurma(int turmaId, int academiaId)
        {
            var turmaExiste = _context.Set<Turma>()
                .Any(t => t.Id == turmaId && t.AcademiaId == academiaId);

            if (!turmaExiste)
            {
                throw new InvalidOperationException("Turma nao encontrada.");
            }
        }

        private void ValidarAluno(int alunoId, int academiaId)
        {
            var alunoExiste = _context.Alunos
                .Any(a => a.Id == alunoId && a.AcademiaId == academiaId);

            if (!alunoExiste)
            {
                throw new InvalidOperationException("Aluno nao encontrado para esta turma.");
            }
        }

        private int? ValidarProfessor(int? professorId, int academiaId)
        {
            if (!professorId.HasValue || professorId.Value <= 0)
            {
                return null;
            }

            var professorExiste = _context.Professores
                .Any(p => p.Id == professorId.Value && p.AcademiaId == academiaId);

            if (!professorExiste)
            {
                throw new InvalidOperationException("Professor nao encontrado para esta turma.");
            }

            return professorId.Value;
        }

        private static string ObterNomeDiaSemana(DateTime data)
        {
            return data.DayOfWeek switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Segunda",
                DayOfWeek.Tuesday => "Terca",
                DayOfWeek.Wednesday => "Quarta",
                DayOfWeek.Thursday => "Quinta",
                DayOfWeek.Friday => "Sexta",
                DayOfWeek.Saturday => "Sabado",
                _ => string.Empty
            };
        }
    }
}
