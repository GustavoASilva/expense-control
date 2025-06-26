using System;

namespace ExpenseControl.Frontend.Models.ApiResponses;

public class TransactionApiResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public Guid CategoryId { get; set; }
    public CategoryApiResponse Category { get; set; }
    public TransactionType Type { get; set; }
    public string Notes { get; set; }
}
