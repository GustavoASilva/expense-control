using System;

namespace ExpenseControl.Frontend.Models
{
    public class BudgetUsageModel
    {
        public decimal Amount { get; set; }
        public decimal Usage { get; set; }
        public decimal Percent { get; set; }
    }
}
