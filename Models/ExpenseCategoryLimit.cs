namespace ExpenseTracker.Models
{
    public class ExpenseCategoryLimit
    {
        public int Id { get; set; }
        public string Category { get; set; }  
        public decimal LimitAmount { get; set; } 

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
