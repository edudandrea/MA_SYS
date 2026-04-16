using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Matriculas;
using MA_Sys.API.Models;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class MatriculaService
    {
        private readonly IMatriculaRepository _repo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IPlanosRepository _planoRepo;
        private readonly IFormaPagamentoRepository _formaPgtoRepo;
        private readonly IPagamentoRepository _pagamentoRepo;

        public MatriculaService(IMatriculaRepository repo, 
                                IAlunoRepository alunoRepo, 
                                IPlanosRepository planoRepo, 
                                IFormaPagamentoRepository formaPgtoRepo,
                                IPagamentoRepository pagamentoRepo)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
            _planoRepo = planoRepo;
            _formaPgtoRepo = formaPgtoRepo;
            _pagamentoRepo = pagamentoRepo;
        }

        private List<Pagamentos> GerarPagamentos(Matricula matricula, Plano plano, FormaPagamento formaPgto, int formaPgtoId)
        {
            var lista = new List<Pagamentos>();
            var valorParcela = plano.Valor / formaPgto.Parcelas;

            for (int i = 0; i < formaPgto.Parcelas; i++)
            {
                var vencimento = matricula.DataInicio.AddDays(formaPgto.Dias * i);
                lista.Add(new Pagamentos
                {
                    AcademiaId = matricula.AcademiaId,
                    AlunoId = matricula.AlunoId,
                    PlanoId = matricula.PlanoId,
                    DataVencimento = vencimento,
                    Valor = valorParcela,
                    Status = "Pendente",
                    FormaPagamentoId = formaPgtoId
                });
            }
            return lista;
        }

        public List<MatriculasResponseDto> List(int academiaId)
        {
            var modalidade = _repo.Query();

            modalidade = modalidade.Where(m => m.AcademiaId == academiaId);

            return modalidade.Select(a => new MatriculasResponseDto
            {
                Id = a.Id,
                AlunoId = a.AlunoId,
                PlanoId = a.PlanoId,
                DataInicio = a.DataInicio,
                DataFim = a.DataFim,
                Status = a.Status
            }).ToList();
        }

        public async Task<Matricula> CreateMatricula(MatriculasCreateDto dto)
        {
            var plano = _planoRepo.Query().FirstOrDefault(p => p.Id == dto.PlanoId);
            if (plano == null)
                throw new Exception("Plano não encontrado");

            var formaPgto = _formaPgtoRepo.Query().FirstOrDefault(f => f.Id == dto.FormaPgtoId);
            if (formaPgto == null)
                throw new Exception("Forma de Pagamento não encontrada");

            var matricula = new Matricula
            {
                AlunoId = dto.AlunoId,
                PlanoId = dto.PlanoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddMonths(plano.DuracaoMeses),
                Status = "Ativa"
            };

            _repo.Add(matricula);
            _repo.Save();

            var parcelas = GerarPagamentos(matricula, plano, formaPgto, dto.FormaPgtoId);

            foreach (var p in parcelas)
                _pagamentoRepo.Add(p);

            _formaPgtoRepo.Save();
            return matricula;

        }
    }
}