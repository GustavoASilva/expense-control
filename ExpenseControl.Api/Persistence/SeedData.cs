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
                    Id = new Guid("a48cde78-354e-4d5c-9159-cf28368fcaca"),
                    Name = "Housing",
                    Description = "Rent, mortgage, repairs, etc.",
                    Type = TransactionType.Expense,
                    IconName = "house"
                },
                new Category
                {
                    Id = new Guid("eb318b9d-aafc-421d-8041-64c6889ffc3c"),
                    Name = "Transportation",
                    Description = "Car, public transit, fuel, etc.",
                    Type = TransactionType.Expense,
                    IconName = "car-front"
                },
                new Category
                {
                    Id = new Guid("c7e8d65f-f4b2-4162-ba57-04f52fb16d51"),
                    Name = "Food",
                    Description = "Groceries, dining out, etc.",
                    Type = TransactionType.Expense,
                    IconName = "cart"
                },
                new Category
                {
                    Id = new Guid("ac800e65-9ae2-4274-8c8a-ae4658345c99"),
                    Name = "Utilities",
                    Description = "Electricity, water, internet, etc.",
                    Type = TransactionType.Expense,
                    IconName = "lightning"
                },
                new Category
                {
                    Id = new Guid("92974cbf-e03c-4f33-9e36-9f9f0a97523e"),
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
                    Id = new Guid("3feb665e-56c5-4251-bda6-d665dbda65d3"),
                    Name = "Salary",
                    Description = "Regular employment income",
                    Type = TransactionType.Income,
                    IconName = "wallet2"
                },
                new Category
                {
                    Id = new Guid("51630c3e-35be-4b55-87a9-68ef640f772c"),
                    Name = "Freelance",
                    Description = "Independent contractor income",
                    Type = TransactionType.Income,
                    IconName = "briefcase"
                },
                new Category
                {
                    Id = new Guid("683eba8b-531b-465e-8be4-584e85abdfb8"),
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
