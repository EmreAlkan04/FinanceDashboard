using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FinanceDashboard.Model
{
    /// <summary>
    /// What a caller sends to create (POST) or edit (PUT) an expense.
    /// Deliberately has no Id: the database assigns it, and for PUT it comes from the URL.
    ///
    /// The attributes are the single source of validation rules: ASP.NET checks them
    /// before the controller runs, and ExpenseService checks them again so that other
    /// callers (e.g. a Razor page) can't bypass them.
    /// </summary>
    public class ExpenseDTO
    {
        // Nullable + [Required] so that a missing value is rejected instead of silently becoming 0001-01-01 / 0.
        [Required]
        public DateTime? Date { get; set; }

        // Keep the limits in sync with FinanceDbContext (category 100, note 500).
        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required, Range(0, double.MaxValue)]
        public double? Expense { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [Required]
        public byte[]? Image { get; set; }
    }

}
