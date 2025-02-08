namespace ExpenseTracker.Models
{
    public class FundsRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public decimal Amount { get; set; }
    }
}