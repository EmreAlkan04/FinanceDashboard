using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace FinanceDashboard.Client.DTO
{
    public class NewExpenseDTO
    {
        [Required]
        public string Category { get; set; } = string.Empty;

        [Range(typeof(double), "0", "1000000")]
        public double Expense { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

    }
}
