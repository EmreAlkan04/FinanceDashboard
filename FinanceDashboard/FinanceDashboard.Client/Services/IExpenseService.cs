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
        List<ExpensesModel> GetFirst(int count);

        /// <summary>Total number of expenses in the database.</summary>
        int GetCount();

        double TotalExpenseOfTheYear(int? year = null);

        List<CategoryCostDTO> ExpenseByCategory(int? year = null);
        List<double> MonthlyExpense(int? year = null);
        double PercentageConverter(string category, double expenseOfCategory, int? year = null);
        List<FinanceDashboard.Model.ExpensesModel> Ranking(double? minExp = 00.00, int? year = null);

        /// <summary>
        /// Returns a List of all entries from the current year.
        /// </summary>
        /// <returns></returns>
        List<ExpensesModel> AllExpensesOfTheYear();
        Task<List<int>> GetAllYears();
        ExpensesModel? GetById(int id);
        ExpensesModel AddExpense(ExpenseDTO expenseDTO);
        ExpensesModel? UpdateExpense(int id, ExpenseDTO expenseDTO);
        Task<int> ImageReceiverAsync(CancellationToken cancellationToken = default);
    }
}
