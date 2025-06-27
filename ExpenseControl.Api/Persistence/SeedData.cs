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
                    IconName = "house"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Transportation",
                    Description = "Car, public transit, fuel, etc.",
                    Type = TransactionType.Expense,
                    IconName = "car-front"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Food",
                    Description = "Groceries, dining out, etc.",
                    Type = TransactionType.Expense,
                    IconName = "cart"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Utilities",
                    Description = "Electricity, water, internet, etc.",
                    Type = TransactionType.Expense,
                    IconName = "lightning"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Healthcare",
                    Description = "Medical expenses, insurance, etc.",
                    Type = TransactionType.Expense,
                    IconName = "heart-pulse"
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
                    IconName = "wallet2"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Freelance",
                    Description = "Independent contractor income",
                    Type = TransactionType.Income,
                    IconName = "briefcase"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Investments",
                    Description = "Dividends, interest, capital gains",
                    Type = TransactionType.Income,
                    IconName = "graph-up-arrow"
                }
            };

            modelBuilder.Entity<Category>().HasData(expenseCategories);
            modelBuilder.Entity<Category>().HasData(incomeCategories);
        }
    }
}
