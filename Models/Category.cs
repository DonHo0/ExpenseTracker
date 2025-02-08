namespace ExpenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal BudgetLimit { get; set; }

        public required ICollection<Expense> Expenses { get; set; }
    }

}
