using System;

namespace ExpenseControl.Frontend.Models.ApiResponses;

public class CategoryApiResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TransactionType Type { get; set; }
    public string? IconName { get; set; }
}
