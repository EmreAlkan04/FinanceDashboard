using FinanceDashboard.Client.Services;
using FinanceDashboard.Model;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboard.Data
{
    public class ExpenseService : IExpenseService
    {
        private readonly FinanceDbContext _db;

        public ExpenseService(FinanceDbContext db)
        {
            _db = db;
        }

        public Task<List<CategoryModel>> GetFirstAsync(int count) =>
            _db.Expenses
               .AsNoTracking()
               .OrderBy(e => e.Id)
               .Take(count)
               .ToListAsync();

        public Task<int> GetCountAsync() => _db.Expenses.CountAsync();


        public double TotalCost(int? year = null)
        {
            year ??= DateTime.Now.Year;

            double totalExpense = 0;

            
            foreach (var item in _db.Expenses.Where(s => s.Date.Year.Equals(year)))
            {
                // var testname = item.Split(",", 4);

                double expense = Convert.ToDouble(item.Expense.ToString().Replace(".", ","));
                // Int32.TryParse(testname.GetValue(2).ToString(), out expense);
                totalExpense += expense;
            }

            totalExpense = Math.Round(totalExpense, 2);

            return totalExpense;
        }

    }
}
