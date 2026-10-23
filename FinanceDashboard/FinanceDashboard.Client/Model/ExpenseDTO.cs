using System.ComponentModel.DataAnnotations;

namespace FinanceDashboard.Model
{
    /// <summary>
    /// What a caller sends to create (POST) or edit (PUT) an expense.
    /// Deliberately has no Id: the database assigns it, and for PUT it comes from the URL.
    /// </summary>
    public class ExpenseDTO
    {
        public ExpenseDTO(string category, double expense)
        {
            Category = category;
            Expense = expense;
        }

        public DateTime Date { get; set; }

        // Keep the limits in sync with FinanceDbContext (category 100, note 500).
        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Expense { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}