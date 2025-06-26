using System;
using System.Collections.Generic;
using ExpenseControl.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Persistence
{
    public static class SeedData
    {
        public static void ConfigureSeedData(this ModelBuilder modelBuilder)
        {
            // Expense Categories
            var expenseCategories = new[]
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Housing",
                    Description = "Rent, mortgage, repairs, etc.",
                    Type = TransactionType.Expense,
                    IconName = "house",
                    ColorCode = "#FF8C00"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Transportation",
                    Description = "Car, public transit, fuel, etc.",
                    Type = TransactionType.Expense,
                    IconName = "car-front",
                    ColorCode = "#4169E1"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Food",
                    Description = "Groceries, dining out, etc.",
                    Type = TransactionType.Expense,
                    IconName = "cart",
                    ColorCode = "#32CD32"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Utilities",
                    Description = "Electricity, water, internet, etc.",
                    Type = TransactionType.Expense,
                    IconName = "lightning",
                    ColorCode = "#FFD700"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Healthcare",
                    Description = "Medical expenses, insurance, etc.",
                    Type = TransactionType.Expense,
                    IconName = "heart-pulse",
                    ColorCode = "#FF69B4"
                }
            };

            // Income Categories
            var incomeCategories = new[]
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Salary",
                    Description = "Regular employment income",
                    Type = TransactionType.Income,
                    IconName = "wallet2",
                    ColorCode = "#228B22"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Freelance",
                    Description = "Independent contractor income",
                    Type = TransactionType.Income,
                    IconName = "briefcase",
                    ColorCode = "#4682B4"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Investments",
                    Description = "Dividends, interest, capital gains",
                    Type = TransactionType.Income,
                    IconName = "graph-up-arrow",
                    ColorCode = "#9370DB"
                }
            };

            modelBuilder.Entity<Category>().HasData(expenseCategories);
            modelBuilder.Entity<Category>().HasData(incomeCategories);
        }
    }
}
