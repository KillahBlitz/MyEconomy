using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<CategoriaModel> Categorias { get; set; }
        public DbSet<SaldoModel> Saldos { get; set; }
        public DbSet<DeudaModel> Deudas { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }
        public DbSet<ProductoModel> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TicketModel>().ToTable(tb => tb.HasTrigger("trg_Ticket_Insert"));
            modelBuilder.Entity<TicketModel>().ToTable(tb => tb.HasTrigger("trg_Ticket_Delete"));
        }
    }
}