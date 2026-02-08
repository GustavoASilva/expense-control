namespace ExpenseControl.Api.Entities;

public class Income : Transaction
{
    public Income()
    {
        Type = TransactionType.Income;
    }
}
