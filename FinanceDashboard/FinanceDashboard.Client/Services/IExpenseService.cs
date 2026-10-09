using FinanceDashboard.Model;

namespace FinanceDashboard.Client.Services
{
    /// <summary>
    /// Read access to the expenses stored in the database.
    /// Defined here so pages in the client project can use it; the implementation
    /// (which needs EF Core / SQL Server) lives in the server project.
    /// </summary>
    public interface IExpenseService
    {
        /// <summary>The first <paramref name="count"/> expenses, ordered by Id.</summary>
        Task<List<CategoryModel>> GetFirstAsync(int count);

        /// <summary>Total number of expenses in the database.</summary>
        Task<int> GetCountAsync();

        double TotalCost(int? year = null);
    }
}
