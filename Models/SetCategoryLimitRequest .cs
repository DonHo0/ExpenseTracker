
namespace ExpenseTracker.Models
{
    public class SetCategoryLimitRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Category { get; set; }
        public decimal LimitAmount { get; set; }
    }
}