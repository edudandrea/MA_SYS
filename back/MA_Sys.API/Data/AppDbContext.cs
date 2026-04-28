using MA_Sys.API.Models;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;


namespace MA_SYS.Api.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options,
                            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Users> User { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Plano> Planos { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Pagamentos> Pagamentos { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<Academia> Academias { get; set; }
        public DbSet<Modalidade> Modalidades { get; internal set; }
        public DbSet <FormaPagamento> FormaPagamentos { get; set; }   
        public DbSet<Matricula> Matriculas { get; set; }     
        public DbSet<PagamentoAcademia> PagamentosAcademias { get; set; }
        public DbSet<Financeiro> Financeiros { get; set; }
        public DbSet<CategoriaTransacao> CategoriasTransacao { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.CreatedByUser)
                .WithMany(u => u.CreatedUsers)
                .HasForeignKey(u => u.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Academia>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
