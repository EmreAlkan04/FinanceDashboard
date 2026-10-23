namespace FinanceDashboard.Model
{
    public class ExpensesModel
    {

        public ExpensesModel() { }
        public ExpensesModel(string category, double expense)
        {
            Category = category;
            Expense = expense;
        }

        public ExpensesModel(string category, double expense, DateTime date, string? note)
        {
            Category = category;
            Expense = expense;
            Date = date;
            Note = note;
        }

        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public double Expense { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}
