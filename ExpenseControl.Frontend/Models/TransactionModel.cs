using System.ComponentModel.DataAnnotations;
using System;

namespace ExpenseControl.Frontend.Models;

public class TransactionModel
{
    public Guid Id { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 200 characters")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }

    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "Please select a category")]
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public CategoryModel? Category { get; set; }

    public string? Notes { get; set; }
}
