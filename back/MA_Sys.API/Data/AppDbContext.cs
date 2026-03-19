using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_SYS.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        public DbSet<User> Users { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Plano> Planos { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Mensalidade> Mensalidades { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<Academia> Academias { get; set; }
        public DbSet<Modalidade> Modalidades { get; internal set; }
    }
}