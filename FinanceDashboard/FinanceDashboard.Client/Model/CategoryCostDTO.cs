namespace FinanceDashboard.Model
{
    /// <summary>Total cost of one category, as returned by GET api/expenses/categories.</summary>
    public record CategoryCostDTO(string Category, double Expense);
}
