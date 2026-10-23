using FinanceDashboard.Client.Services;
using FinanceDashboard.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace FinanceDashboard.Data
{
    public class ExpenseService : IExpenseService
    {
        private readonly FinanceDbContext _db;

        public ExpenseService(FinanceDbContext db)
        {
            _db = db;
        }

        public List<ExpensesModel> GetFirst(int count) =>
            _db.Expenses
               .AsNoTracking()
               .OrderBy(e => e.Id)
               .Take(count)
               .ToList();

        public int GetCount() => _db.Expenses.Count();

        /// <summary>
        /// Returns a List of all entries from the current year.
        /// </summary>
        /// <returns></returns>
        public List<ExpensesModel> AllExpensesOfTheYear() =>
            _db.Expenses.Where(s => s.Date.Year.Equals(DateTime.Now.Year)).ToList();


        public double TotalExpenseOfTheYear(int? year = null)
        {
            year ??= DateTime.Now.Year;

            double totalExpense = 0;


            foreach (var entriesOfYear in _db.Expenses.Where(s => s.Date.Year.Equals(year)))
            {
                // var testname = item.Split(",", 4);

                double expense = Convert.ToDouble(entriesOfYear.Expense.ToString().Replace(".", ","));
                // Int32.TryParse(testname.GetValue(2).ToString(), out expense);
                totalExpense += expense;
            }

            totalExpense = Math.Round(totalExpense, 2);

            return totalExpense;
        }

        public List<FinanceDashboard.Model.ExpensesModel> ExpenseByCategory(int? year = null)
        {
            year ??= DateTime.Now.Year;

            HashSet<string> catNames = new HashSet<string>();
            List<FinanceDashboard.Model.ExpensesModel> list = new List<FinanceDashboard.Model.ExpensesModel>();

            foreach (var item in _db.Expenses.Where(s => s.Date.Year.Equals(year)))
            {
                // string cat = item.Split(",", 3)[1];
                if (!catNames.Any(s => s.Equals(item.Category.ToString())))
                    catNames.Add(item.Category);
            }

            foreach (var categoryItem in catNames)
            {
                double catExpense = 0;
                var catEntries = _db.Expenses.Where(s => s.Category.Equals(categoryItem) && s.Date.Year.Equals(year)).ToList();

                foreach (var item in catEntries)
                {
                    catExpense += item.Expense;

                    //var testname = item.Split(",", 4);

                    //double expense = Convert.ToDouble(testname.GetValue(2).ToString().Replace(".", ","));
                    //// Int32.TryParse(testname.GetValue(2).ToString(), out expense);
                    //catExpense += expense;
                }

                catExpense = Math.Round(catExpense, 2);
                list.Add(new Model.ExpensesModel(categoryItem, catExpense));
            }

            list = list.OrderByDescending(s => s.Expense).ToList();

            return list;
        }

        public List<double> MonthlyExpense(int? year = null)
        {
            year ??= DateTime.Now.Year;

            DateTime target = new DateTime(year ?? DateTime.Now.Year, 1, 1);
            List<double> montlyExpensesList = new List<double>();

            for (int i = 1; i <= 12; i++)
            {
                var monthEntries = _db.Expenses.Where(s => s.Date.Year.Equals(target.Year) && s.Date.Month.Equals(target.AddMonths(i - 1).Month));
                double monthlyExpense = Math.Round(monthEntries.Sum(s => s.Expense), 2);
                montlyExpensesList.Add(monthlyExpense);
            }

            //for (var i = 1; i <= DateTime.Now.Month; i++)
            //{
            //    double montly = 0.0;
            //    foreach (var montlyExpense in _db.Expenses.Where(s => s.Date.Year.Equals(target.Year) && s.Date.Month.Equals(target.AddMonths(i - 1).Month)))
            //    {
            //        // var testname = montlyExpense.Split(",", 4);

            //        double expense = Convert.ToDouble(montlyExpense.Expense.ToString().Replace(".", ","));
            //        // Int32.TryParse(testname.GetValue(2).ToString(), out expense);
            //        montly += expense;

            //    }
            //    montly = Math.Round(montly, 2);

            //    montlyExpensesList.Add(montly);
            //}

            return montlyExpensesList;
        }

        public List<FinanceDashboard.Model.ExpensesModel> PercentageChart()
        {

            List<FinanceDashboard.Model.ExpensesModel> percentages = new List<FinanceDashboard.Model.ExpensesModel>();
            var total = TotalExpenseOfTheYear();

            foreach (var item in ExpenseByCategory())
            {
                var irtgendwas = (item.Expense / total) * 100;
                irtgendwas = Math.Round(irtgendwas, 2);
                FinanceDashboard.Model.ExpensesModel model = new Model.ExpensesModel(item.Category, irtgendwas);
                percentages.Add(model);
            }

            return percentages;
        }

        public List<FinanceDashboard.Model.ExpensesModel> Ranking(double? minExp = 00.00, int? year = null)
        {
            year ??= DateTime.Now.Year;

            List<FinanceDashboard.Model.ExpensesModel> ranked = new List<FinanceDashboard.Model.ExpensesModel>();

            foreach (var item in _db.Expenses.Where(s => s.Date.Year.Equals(year) && s.Expense >= minExp && !string.IsNullOrEmpty(s.Note)).OrderByDescending(s => s.Expense))
                ranked.Add(item);

            return ranked;
        }

        public Task<List<int>> GetAllYears()
        {
            return _db.Expenses.Select(s => s.Date.Year).Distinct().ToListAsync();
        }
    }
}
