using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ExpenseTracker.Data;
using ExpenseTracker.Models;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ExpenseTrackerContext _context;

    public UsersController(ExpenseTrackerContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Invalid user data.");
        }

        // Set available budget equal to the initial budget.
        user.AvailableBudget = user.InitialBudget;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> GetUserExpenses([FromBody] UserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Expenses)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(user.Expenses);
    }

    [HttpPost("addfunds")]
    public async Task<IActionResult> AddFunds([FromBody] FundsRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        user.AvailableBudget += request.Amount;
        await _context.SaveChangesAsync();

        return Ok($"Funds added. New available budget: {user.AvailableBudget}");
    }

    [HttpPost("setlimit")]
    public async Task<IActionResult> SetCategoryLimit([FromBody] SetCategoryLimitRequest request)
    {
        var user = await _context.Users
            .Include(u => u.CategoryLimits)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        var existingLimit = user.CategoryLimits.FirstOrDefault(c => c.Category == request.Category);
        if (existingLimit != null)
        {
            existingLimit.LimitAmount = request.LimitAmount;
        }
        else
        {
            var newLimit = new ExpenseCategoryLimit
            {
                Category = request.Category,
                LimitAmount = request.LimitAmount,
                UserId = user.Id
            };
            _context.ExpenseCategoryLimits.Add(newLimit);
        }

        await _context.SaveChangesAsync();
        return Ok("Category limit set/updated successfully.");
    }

    [HttpGet("allusers")]
    public async Task<IActionResult> GetAllUsersWithExpenses()
    {
        var users = await _context.Users
            .Include(u => u.Expenses)
            .Include(u => u.CategoryLimits) 
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.AvailableBudget,
                Expenses = u.Expenses.Select(e => new
                {
                    e.Id,
                    e.Amount,
                    e.Category,
                    e.Date
                }),
                CategoryLimits = u.CategoryLimits.Select(cl => new
                {
                    cl.Id,
                    cl.Category,
                    cl.LimitAmount
                })
            })
            .ToListAsync();

        if (users == null || !users.Any())
        {
            return NotFound("No users found.");
        }

        return Ok(users);
    }

    [HttpGet("highestexpenses")]
    public async Task<IActionResult> GetUserWithHighestTotalExpenses()
    {
        var userWithHighestExpenses = await _context.Users
            .Where(u => u.Expenses.Any()) // Ensure user has expenses
            .Select(u => new
            {
                UserId = u.Id,
                Username = u.Username,
                TotalExpenses = u.Expenses.Sum(e => (decimal?)e.Amount) ?? 0  // Handle null cases
            })
            .OrderByDescending(u => u.TotalExpenses)
            .FirstOrDefaultAsync();

        if (userWithHighestExpenses == null)
        {
            return NotFound("No users found with expenses.");
        }

        return Ok(userWithHighestExpenses);
    }


}
