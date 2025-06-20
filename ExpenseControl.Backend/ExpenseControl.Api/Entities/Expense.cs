namespace ExpenseControl.Api.Entities
{
    public class Expense : Transaction
    {
        public Expense()
        {
            Type = TransactionType.Expense;
        }
    }
}
