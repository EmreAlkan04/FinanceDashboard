using FinanceDashboard.Model;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboard.Data
{
    public class FinanceDbContext : DbContext
    {

        public const int CategoryNameMaxLength = 100;
        public const int NoteMaxLength = 500;

        public FinanceDbContext(
            DbContextOptions<FinanceDbContext> options) : base(options)
        {

        }

        public DbSet<ExpensesModel> Expenses => Set<ExpensesModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExpensesModel>(entity =>
            {
                entity.ToTable("Expenses");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Category)
                      .IsRequired()
                      .HasMaxLength(CategoryNameMaxLength);

                entity.Property(e => e.Note)
                      .HasMaxLength(NoteMaxLength);

                entity.Property(e => e.Date)
                      .HasColumnType("date");

                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Category);

                //entity.Property(e => e.Image)
                //    .HasColumnType("varbinary(max)")
                //    .IsRequired(false);
            });
        }
    }
}
