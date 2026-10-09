namespace FinanceDashboard.Model
{
    public class CategoryModel
    {

        public CategoryModel() { }
        public CategoryModel(string categoryName, double expense)
        {
            CategoryName = categoryName;
            Expense = expense;
        }

        public CategoryModel(string categoryName, double expense, DateTime date, string? note)
        {
            CategoryName = categoryName;
            Expense = expense;
            Date = date;
            Note = note;
        }

        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double Expense { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}
