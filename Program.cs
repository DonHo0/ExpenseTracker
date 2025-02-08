using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// Add connection string
var connectionString = builder.Configuration.GetConnectionString("ExpenseTracker");

builder.Services.AddDbContext<ExpenseTrackerContext>(options =>
    options.UseSqlServer(connectionString));

// Add Controllers service
builder.Services.AddControllers();

// Add Authorization services
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware setup
app.UseRouting();

// Add UseAuthorization() after routing
app.UseAuthorization();

// Map controllers for routing
app.MapControllers();

app.Run();
