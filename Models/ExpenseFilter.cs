namespace ExpenseTracker.Models
{
    public class ExpenseFilter
    {
        public string? Category { get; set; }  
        public DateTime? FromDate { get; set; }  
        public DateTime? ToDate { get; set; }  
    }
}