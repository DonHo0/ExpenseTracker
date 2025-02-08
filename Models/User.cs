namespace ExpenseTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public decimal InitialBudget { get; set; }
        public decimal AvailableBudget { get; set; }

        public List<Expense> Expenses { get; set; } = new List<Expense>();
        public List<ExpenseCategoryLimit> CategoryLimits { get; set; } = new List<ExpenseCategoryLimit>();
    }
}