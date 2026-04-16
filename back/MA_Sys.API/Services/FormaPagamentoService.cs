using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.FormaPagamentos;
using MA_Sys.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class FormaPagamentoService
    {
        private readonly IFormaPagamentoRepository _repo;

        public FormaPagamentoService(IFormaPagamentoRepository repo)
        {
            _repo = repo;
        }

        public List<FormaPagamentoResponseDto> List(int academiaId)
        {
            var modalidade = _repo.Query();

            modalidade = modalidade.Where(m => m.AcademiaId == academiaId);

            return modalidade.Select(a => new FormaPagamentoResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Ativo = a.Ativo,
                Taxa = a.Taxa,
                Parcelas = a.Parcelas,
                Dias = a.Dias
            }).ToList();
        }

        public List<FormaPagamentoResponseDto> Get(string role, FormaPagamentoFiltroDto filtro, int? academiaId)
        {
            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia não pode acessar formas de pagamento");

                query = query.Where(m => m.AcademiaId == academiaId);
            }


            if (filtro != null && !string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(m => m.Nome.Contains(filtro.Nome));

            return query.Select(m => new FormaPagamentoResponseDto
            {
                Id = m.Id,
                Nome = m.Nome,
                Ativo = m.Ativo,
                Taxa = m.Taxa,
                Parcelas = m.Parcelas,
                Dias = m.Dias
            }).ToList();
        }



        public void Add(FormaPagamentoCreateDto dto, int? academiaId, string role)
        {
            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuário sem vinculo com academia não pode criar formas de pagamento");

                query = query.Where(m => m.AcademiaId == academiaId);
            }
            
            var formaPagamento = new FormaPagamento
            {
                Nome = dto.Nome,
                AcademiaId = academiaId ?? 0,
                Ativo = true
            };

            _repo.Add(formaPagamento);
            _repo.Save();
        }

        public void Update(int id, FormaPagamentoUpdateDto dto)
        {
            var formaPagamento = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if (formaPagamento == null)
                throw new Exception("Forma de pagamento não encontrada");

            formaPagamento.Nome = dto.Nome;
            formaPagamento.Ativo = dto.Ativo;
            formaPagamento.Taxa = dto.Taxa;
            formaPagamento.Parcelas = dto.Parcelas;
            formaPagamento.Dias = dto.Dias;

            _repo.Save();
        }

        public void UpdateStatus(int id, int? academiaId, bool ativo)
        {
            var modalidade = _repo.GetById(id, academiaId ?? 0);

            if (modalidade == null)
                throw new Exception("Modalidade não encontrado");

            modalidade.Ativo = ativo;

            _repo.Update(modalidade);
            _repo.Save();
        }


    }
}