using FinanceDashboard.Client.Services;
using FinanceDashboard.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SerpApi;
using System.Web;
using System.Security.Policy;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FinanceDashboard.Data
{
    public class ExpenseService : IExpenseService
    {
        private readonly FinanceDbContext _db;
        private readonly IConfiguration configuration;
        private readonly ImageSettings _imageSettings;

        public ExpenseService(FinanceDbContext db, IConfiguration configuration, ImageSettings imageSettings)
        {
            _db = db;
            this.configuration = configuration;
            _imageSettings = imageSettings;
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
                    catExpense += item.Expense;

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

            return _db.Expenses
                    .Where(s => s.Date.Year == year
                                && s.Expense >= minExp
                                && !string.IsNullOrEmpty(s.Note))
                    .OrderByDescending(s => s.Expense)
                    .ToList();
        }

        public Task<List<int>> GetAllYears()
        {
            return _db.Expenses.Select(s => s.Date.Year).Distinct().ToListAsync();
        }

        public ExpensesModel? GetById(int id) =>
            _db.Expenses.AsNoTracking().FirstOrDefault(e => e.Id == id);

        public ExpensesModel AddExpense(ExpenseDTO dto)
        {
            Validate(dto);

            var expense = new ExpensesModel();
            CopyValues(dto, expense);

            _db.Expenses.Add(expense);
            _db.SaveChanges(); // a DbUpdateException is not caught here; the API's exception filter reports it

            return expense;
        }

        public ExpensesModel? UpdateExpense(int id, ExpenseDTO dto)
        {
            Validate(dto);

            var expense = _db.Expenses.Find(id);
            if (expense is null)
            {
                return null;
            }

            CopyValues(dto, expense);
            _db.SaveChanges();

            return expense;
        }

        private static void Validate(ExpenseDTO dto) =>
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

        // Only called after Validate(), so Date and Expense are known to have a value.
        private static void CopyValues(ExpenseDTO dto, ExpensesModel expense)
        {
            expense.Category = dto.Category.Trim();
            expense.Expense = Math.Round(dto.Expense!.Value, 2);
            expense.Date = dto.Date!.Value.Date; //no time needed
            expense.Note = NormalizeNote(dto.Note);
        }

        // An empty note and the text "null" (as written in the CSV) are stored as NULL.
        private static string? NormalizeNote(string? note)
        {
            var trimmed = note?.Trim();

            return string.IsNullOrEmpty(trimmed) || trimmed.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : trimmed;
        }

        public async Task<int> ImageReceiverAsync(
            CancellationToken cancellationToken = default)
        {
            var apiKey = configuration["SerpApi:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("SerpApi API key is missing.");

            var allowedCategories = _imageSettings.AllowedCategories;

            var entriesWithoutImage = await _db.Expenses
                .Where(s => s.Expense >= _imageSettings.MinimumExpense
                            && s.Note != null
                            && s.Note != ""
                            && s.Image == null 
                            && allowedCategories.Contains(s.Category))
                .ToListAsync(cancellationToken);

            if (entriesWithoutImage.Count == 0)
                return 0;

            using var client = new SerpApiClient(apiKey);
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            int updatedCount = 0;

            foreach (var item in entriesWithoutImage)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var parameters = new Dictionary<string, string>
                    {
                        ["engine"] = "google_images",
                        ["q"] = item.Note!,
                        ["location"] = "Wien-Flughafen, Lower Austria, Austria",
                        ["google_domain"] = "google.at",
                        ["hl"] = "de",
                        ["gl"] = "at"
                    };

                    using var results = await client.SearchAsync(parameters);

                    if (!TryGetImageUrl(results.Root, out string? imageUrl))
                        continue;

                    byte[] imageBytes = await httpClient.GetByteArrayAsync(
                        imageUrl,
                        cancellationToken);

                    if (imageBytes.Length == 0)
                        continue;

                    // The entity is already tracked by EF Core.
                    item.Image = imageBytes;
                    updatedCount++;
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // A failed request should not stop processing other expenses.
                    Console.WriteLine(
                        $"Could not retrieve image for expense {item.Id}: {ex.Message}");
                }
            }

            if (updatedCount > 0)
                await _db.SaveChangesAsync(cancellationToken);

            return updatedCount;
        }

        private static bool TryGetImageUrl(
            JsonElement root,
            out string? imageUrl)
        {
            imageUrl = null;

            if (!root.TryGetProperty("images_results", out var images)
                || images.ValueKind != JsonValueKind.Array
                || images.GetArrayLength() == 0)
            {
                return false;
            }

            var firstImage = images[0];

            if (!firstImage.TryGetProperty("original", out var original)
                || original.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var candidate = original.GetString();

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttps
                && uri.Scheme != Uri.UriSchemeHttp)
            {
                return false;
            }

            imageUrl = uri.AbsoluteUri;
            return true;
        }
    }
}
