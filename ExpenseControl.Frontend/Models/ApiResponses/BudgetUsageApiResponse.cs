namespace ExpenseControl.Frontend.Models.ApiResponses
{
    public class BudgetUsageApiResponse
    {
        public decimal Amount { get; set; }
        public decimal Usage { get; set; }
        public decimal Percent { get; set; }
    }
}
