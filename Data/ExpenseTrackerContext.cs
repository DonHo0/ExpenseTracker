using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

namespace ExpenseTracker.Data
{
    public class ExpenseTrackerContext : DbContext
    {
        public ExpenseTrackerContext(DbContextOptions<ExpenseTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategoryLimit> ExpenseCategoryLimits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the relationship for CategoryLimits (one-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.CategoryLimits)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
        }
    }
}
