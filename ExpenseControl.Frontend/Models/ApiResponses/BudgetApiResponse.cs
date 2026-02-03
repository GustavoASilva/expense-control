using System;

namespace ExpenseControl.Frontend.Models.ApiResponses
{
    public class BudgetApiResponse
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public CategoryApiResponse Category { get; set; }
        public decimal Amount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
