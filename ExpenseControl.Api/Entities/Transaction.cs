using System;

namespace ExpenseControl.Api.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateOnly Date { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public TransactionType Type { get; set; }
        public string? Notes { get; set; }
        public Guid? HouseholdId { get; set; }
        public Household? Household { get; set; }
    }
}
