using System.Globalization;
using FinanceDashboard.Model;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboard.Data
{
    /// <summary>
    /// Imports wwwroot/expenses.csv into an empty Expenses table.
    /// Safe to call on every startup: it does nothing if the table already has rows.
    /// </summary>
    public static class FinanceDbSeeder
    {
        private const string DateFormat = "yyyy-MM-dd";

        /// <returns>The number of rows that were inserted (0 if the table was already filled or the file is missing).</returns>
        public static async Task<int> SeedFromCsvAsync(FinanceDbContext db, string csvPath, ILogger logger)
        {
            if (await db.Expenses.AnyAsync())
            {
                return 0;
            }

            if (!File.Exists(csvPath))
            {
                logger.LogWarning("CSV import skipped: file not found at {CsvPath}", csvPath);
                return 0;
            }

            var expenses = new List<CategoryModel>();
            var skipped = 0;
            var lineNumber = 0;

            foreach (var line in File.ReadLines(csvPath))
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (TryParseLine(line, out var expense, out var error))
                {
                    expenses.Add(expense);
                }
                else
                {
                    skipped++;
                    logger.LogWarning("CSV line {LineNumber} skipped ({Error}): {Line}", lineNumber, error, line);
                }
            }

            // The CSV is not sorted by date; insert chronologically so the Ids are, too.
            db.Expenses.AddRange(expenses.OrderBy(e => e.Date));
            await db.SaveChangesAsync();

            if (skipped > 0)
            {
                logger.LogWarning("{Skipped} CSV line(s) could not be imported, see warnings above.", skipped);
            }

            return expenses.Count;
        }

        /// <summary>
        /// Parses one line of the form:  2025-01-02, Musik, 70, yousician
        /// </summary>
        private static bool TryParseLine(string line, out CategoryModel expense, out string error)
        {
            expense = null!;
            error = string.Empty;

            // Limit to 4 columns: the note is last and may itself contain commas,
            // e.g. "Spider-Man (50,94 EUR)". Everything after the 3rd comma is the note.
            var parts = line.Split(',', 4);

            if (parts.Length < 4)
            {
                error = "expected 4 columns: date, category, amount, note";
                return false;
            }

            // Every field after the first starts with a space in the file, so trim all of them.
            var dateText = parts[0].Trim();
            var category = parts[1].Trim();
            var amountText = parts[2].Trim();
            var note = parts[3].Trim();

            if (!DateTime.TryParseExact(dateText, DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
            {
                error = $"invalid date '{dateText}', expected {DateFormat}";
                return false;
            }

            if (category.Length == 0 || category.Length > FinanceDbContext.CategoryNameMaxLength)
            {
                error = $"category must be 1-{FinanceDbContext.CategoryNameMaxLength} characters";
                return false;
            }

            // Amounts use '.' as decimal separator, independent of the PC's regional settings.
            if (!double.TryParse(amountText, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount)
                || !double.IsFinite(amount))
            {
                error = $"invalid amount '{amountText}'";
                return false;
            }

            // The file writes "no note" as the text null; store that as a real NULL.
            string? noteValue = note.Length == 0 || note.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : note;

            if (noteValue is { Length: > FinanceDbContext.NoteMaxLength })
            {
                error = $"note longer than {FinanceDbContext.NoteMaxLength} characters";
                return false;
            }

            expense = new CategoryModel(category, amount, date, noteValue);
            return true;
        }
    }
}
