using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ExpenseTracker.Data;
using ExpenseTracker.Models;


[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseTrackerContext _context;

    public ExpensesController(ExpenseTrackerContext context)
    {
        _context = context;
    }

    [HttpPost("addexpense")]
    public async Task<IActionResult> AddExpense([FromBody] ExpenseRequest request)
    {
        if (request == null || request.Expense == null || request.Expense.Amount <= 0)
        {
            return BadRequest("Invalid expense data.");
        }

        var user = await _context.Users
            .Include(u => u.CategoryLimits)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (request.Expense.Amount > user.AvailableBudget)
        {
            return BadRequest("Insufficient available budget.");
        }

        if (!string.IsNullOrEmpty(request.Expense.Category))
        {
            var limit = user.CategoryLimits.FirstOrDefault(l => l.Category == request.Expense.Category);
            if (limit != null && request.Expense.Amount > limit.LimitAmount)
            {
                return BadRequest($"Expense exceeds the limit for category '{request.Expense.Category}' (limit: {limit.LimitAmount}).");
            }
        }

        request.Expense.UserId = user.Id;
        _context.Expenses.Add(request.Expense);
        user.AvailableBudget -= request.Expense.Amount;
        await _context.SaveChangesAsync();

        return Ok("Expense added successfully.");
    }


    [HttpGet("totalexpenses")]
    public async Task<IActionResult> GetTotalExpenses(string category = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Expenses.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(e => e.Category == category);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Date <= toDate.Value);
        }

        var totalAmount = await query.SumAsync(e => e.Amount);

        return Ok(new { TotalExpenses = totalAmount });
    }

    [HttpPost("mostexpensive")]
    public async Task<IActionResult> GetMostExpensiveExpense([FromBody] ExpenseFilter filter)
    {
        if (filter == null ||
         string.IsNullOrEmpty(filter.Category) &&
         !filter.FromDate.HasValue &&
         !filter.ToDate.HasValue)
        {
            return BadRequest("Filter cannot be null or all empty.");
        }

        var query = _context.Expenses.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(e => e.Category == filter.Category);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(e => e.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(e => e.Date <= filter.ToDate.Value);
        }

        var mostExpensive = await query.OrderByDescending(e => e.Amount).FirstOrDefaultAsync();

        if (mostExpensive == null)
        {
            return NotFound("No expenses found matching the criteria.");
        }

        return Ok(mostExpensive);
    }

    [HttpPost("leastexpensive")]
    public async Task<IActionResult> GetLeastExpensiveExpense([FromBody] ExpenseFilter filter)
    {
        if (filter == null ||
        string.IsNullOrEmpty(filter.Category) &&
        !filter.FromDate.HasValue &&
        !filter.ToDate.HasValue)
        {
            return BadRequest("Filter cannot be null or all empty.");
        }

        var query = _context.Expenses.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(e => e.Category == filter.Category);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(e => e.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(e => e.Date <= filter.ToDate.Value);
        }

        var leastExpensive = await query.OrderBy(e => e.Amount).FirstOrDefaultAsync();

        if (leastExpensive == null)
        {
            return NotFound("No expenses found matching the criteria.");
        }

        return Ok(leastExpensive);
    }

    [HttpPost("averagedaily")]
    public async Task<IActionResult> GetAverageDailyExpenses([FromBody] ExpenseFilter filter)
    {
        var query = _context.Expenses.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(e => e.Category == filter.Category);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(e => e.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(e => e.Date <= filter.ToDate.Value);
        }

        var totalDays = (filter.ToDate ?? DateTime.Now).Subtract(filter.FromDate ?? DateTime.Now).Days + 1;
        var totalAmount = await query.SumAsync(e => e.Amount);

        var averageDaily = totalAmount / totalDays;

        return Ok(new { AverageDaily = averageDaily });
    }

    [HttpPost("averagemonthly")]
    public async Task<IActionResult> GetAverageMonthlyExpenses([FromBody] ExpenseFilter filter)
    {
        if (filter == null)
        {
            return BadRequest("Filter cannot be null.");
        }

        var query = _context.Expenses.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(e => e.Category == filter.Category);
        }

        DateTime fromDate = filter.FromDate ?? await _context.Expenses.MinAsync(e => e.Date);
        DateTime toDate = filter.ToDate ?? DateTime.Now;

        if (fromDate > toDate)
        {
            return BadRequest("FromDate cannot be later than ToDate.");
        }

        query = query.Where(e => e.Date >= fromDate && e.Date <= toDate);

        DateTime firstDayOfFromMonth = new DateTime(fromDate.Year, fromDate.Month, 1);
        DateTime lastDayOfToMonth = new DateTime(toDate.Year, toDate.Month, DateTime.DaysInMonth(toDate.Year, toDate.Month));

        int totalMonths = ((lastDayOfToMonth.Year - firstDayOfFromMonth.Year) * 12) + (lastDayOfToMonth.Month - firstDayOfFromMonth.Month) + 1;

        if (totalMonths <= 0)
        {
            totalMonths = 1;
        }

        decimal totalAmount = await query.SumAsync(e => e.Amount);
        decimal averageMonthly = totalAmount / totalMonths;

        return Ok(new { AverageMonthly = Math.Round(averageMonthly, 2) });
    }


    [HttpPost("averageyearly")]
    public async Task<IActionResult> GetAverageYearlyExpenses([FromBody] ExpenseFilter filter)
    {
        if (filter == null)
        {
            return BadRequest("Filter cannot be null.");
        }

        var query = _context.Expenses.AsQueryable();

        DateTime fromDate = filter.FromDate ?? await _context.Expenses.MinAsync(e => e.Date);
        DateTime toDate = filter.ToDate ?? DateTime.Now;

        if (fromDate > toDate)
        {
            return BadRequest("FromDate cannot be later than ToDate.");
        }

        query = query.Where(e => e.Date >= fromDate && e.Date <= toDate);

        var averageYearlyExpenses = await query
            .GroupBy(e => e.Date.Year)
            .Select(g => new
            {
                Year = g.Key,
                AverageExpense = Math.Round(g.Average(e => e.Amount), 2) // Rounded to 2 decimal places
            })
            .ToListAsync();

        if (!averageYearlyExpenses.Any())
        {
            return NotFound("No expenses found in the given period.");
        }

        return Ok(averageYearlyExpenses);
    }


    [HttpGet("mostfrequentcategory")]
    public async Task<IActionResult> GetMostFrequentlyUsedCategory()
    {
        var mostFrequentCategory = await _context.Expenses
            .GroupBy(e => e.Category)
            .OrderByDescending(g => g.Count())
            .Select(g => new
            {
                Category = g.Key,
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        if (mostFrequentCategory == null)
        {
            return NotFound("No categories found.");
        }

        return Ok(mostFrequentCategory);
    }

    [HttpGet("highestaveragedailyspending")]
    public async Task<IActionResult> GetMonthWithHighestAverageDailySpending()
    {
        var monthlyExpenses = await _context.Expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(e => e.Amount) 
            })
            .ToListAsync();

        if (!monthlyExpenses.Any())
        {
            return NotFound("No data available.");
        }

        var monthWithHighestAvg = monthlyExpenses
            .Select(m => new
            {
                m.Year,
                m.Month,
                TotalAmount = m.TotalAmount,
                DaysInMonth = DateTime.DaysInMonth(m.Year, m.Month),
                AverageDailySpending = m.TotalAmount / DateTime.DaysInMonth(m.Year, m.Month)
            })
            .OrderByDescending(m => m.AverageDailySpending)
            .FirstOrDefault();

        return Ok(monthWithHighestAvg);
    }

}