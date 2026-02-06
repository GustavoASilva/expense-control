using System;

namespace ExpenseControl.Api.Entities
{
    public class Household
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
