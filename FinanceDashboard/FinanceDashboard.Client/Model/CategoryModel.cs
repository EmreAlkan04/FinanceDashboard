namespace FinanceDashboard.Model
{
    public class CategoryModel
    {
        public CategoryModel(string categoryName, double expense)
        {
            CategoryName = categoryName;
            Expense = expense;
        }

        public string CategoryName { get; set; }
        public double Expense { get; set; }
    }
}
