namespace ExpenseTracker.Models
{
    public class ExpenseRequest
    {
        public string Username{ get; set; }
        public string Password { get; set; }
        public Expense Expense { get; set; }
    }
}