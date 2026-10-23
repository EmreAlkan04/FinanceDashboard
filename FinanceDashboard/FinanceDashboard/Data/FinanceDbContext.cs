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

                // An expense belongs to a day, not to a point in time.
                entity.Property(e => e.Date)
                      .HasColumnType("date");

                // The dashboard filters by year/month and groups by category.
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Category);
            });
        }
    }
}
