using System;
using System.Collections.Generic;

namespace ExpenseControl.Frontend.Models.ApiResponses;

public class BalanceByCategoryApiResponse
{
    public List<CategoryBalanceApiResponse> Categories { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool HasTransactions { get; set; }
}
