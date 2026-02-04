import React, { useState, useEffect } from 'react';
import { Category, BudgetForm as BudgetFormType, Budget, TransactionType } from '../types/index';
import { getCategories, createBudget } from '../services/api';

interface BudgetFormProps {
  show: boolean;
  onClose: () => void;
  onSave: () => void;
  editBudget?: Budget | null;
}

const BudgetForm: React.FC<BudgetFormProps> = ({ show, onClose, onSave, editBudget }) => {
  const now = new Date();
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [budget, setBudget] = useState<BudgetFormType>({
    categoryId: '',
    amount: 0,
    month: now.getMonth() + 1,
    year: now.getFullYear(),
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (show) {
      loadCategories();
      if (editBudget) {
        setBudget({
          categoryId: editBudget.categoryId,
          amount: editBudget.amount,
          month: editBudget.month,
          year: editBudget.year,
        });
      } else {
        // Reset to defaults for new budget
        const now = new Date();
        setBudget({
          categoryId: '',
          amount: 0,
          month: now.getMonth() + 1,
          year: now.getFullYear(),
        });
      }
      setErrors({});
    }
  }, [show, editBudget]);

  const loadCategories = async () => {
    setIsLoading(true);
    try {
      const data = await getCategories();
      // Filter to expense categories only
      const expenseCategories = data.filter(c => c.type === TransactionType.Expense);
      setCategories(expenseCategories);
    } catch (error) {
      console.error('Failed to load categories:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    let parsedValue: string | number = value;
    
    if (name === 'amount') {
      parsedValue = parseFloat(value) || 0;
    } else if (name === 'month' || name === 'year') {
      parsedValue = parseInt(value) || 0;
    }
    
    setBudget((prev) => ({
      ...prev,
      [name]: parsedValue,
    }));
    // Clear error for this field
    if (errors[name]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[name];
        return newErrors;
      });
    }
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (budget.amount <= 0) {
      newErrors.amount = 'Amount must be greater than 0';
    }

    if (!budget.categoryId) {
      newErrors.categoryId = 'Please select a category';
    }

    if (budget.month < 1 || budget.month > 12) {
      newErrors.month = 'Month must be between 1 and 12';
    }

    if (budget.year < 2000 || budget.year > 2100) {
      newErrors.year = 'Please enter a valid year';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate() || isSaving) {
      return;
    }

    setIsSaving(true);
    try {
      await createBudget(budget);
      onSave();
      onClose();
    } catch (error) {
      console.error('Failed to save budget:', error);
      alert('Failed to save budget. Please try again.');
    } finally {
      setIsSaving(false);
    }
  };

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  if (!show) return null;

  return (
    <>
      <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
        <div className="modal-dialog modal-dialog-centered">
          <div className="modal-content">
            <form onSubmit={handleSubmit}>
              <div className="modal-header">
                <h5 className="modal-title">{editBudget ? 'Edit Budget' : 'New Budget'}</h5>
                <button type="button" className="btn-close" onClick={onClose} aria-label="Close"></button>
              </div>
              <div className="modal-body">
                {isLoading ? (
                  <div className="text-center py-3">
                    <div className="spinner-border text-primary" role="status">
                      <span className="visually-hidden">Loading...</span>
                    </div>
                  </div>
                ) : (
                  <div className="row g-3">
                    <div className="col-12">
                      <label htmlFor="categoryId" className="form-label">
                        Category
                      </label>
                      <select
                        className={`form-select ${errors.categoryId ? 'is-invalid' : ''}`}
                        id="categoryId"
                        name="categoryId"
                        value={budget.categoryId}
                        onChange={handleInputChange}
                        disabled={!!editBudget}
                      >
                        <option value="">Select a category</option>
                        {categories.map((category) => (
                          <option key={category.id} value={category.id}>
                            {category.name}
                          </option>
                        ))}
                      </select>
                      {errors.categoryId && (
                        <div className="invalid-feedback">{errors.categoryId}</div>
                      )}
                    </div>

                    <div className="col-md-6">
                      <label htmlFor="month" className="form-label">
                        Month
                      </label>
                      <select
                        className={`form-select ${errors.month ? 'is-invalid' : ''}`}
                        id="month"
                        name="month"
                        value={budget.month}
                        onChange={handleInputChange}
                        disabled={!!editBudget}
                      >
                        {monthNames.map((name, index) => (
                          <option key={index + 1} value={index + 1}>
                            {name}
                          </option>
                        ))}
                      </select>
                      {errors.month && (
                        <div className="invalid-feedback">{errors.month}</div>
                      )}
                    </div>

                    <div className="col-md-6">
                      <label htmlFor="year" className="form-label">
                        Year
                      </label>
                      <input
                        type="number"
                        className={`form-control ${errors.year ? 'is-invalid' : ''}`}
                        id="year"
                        name="year"
                        value={budget.year}
                        onChange={handleInputChange}
                        min="2000"
                        max="2100"
                        disabled={!!editBudget}
                      />
                      {errors.year && (
                        <div className="invalid-feedback">{errors.year}</div>
                      )}
                    </div>

                    <div className="col-12">
                      <label htmlFor="amount" className="form-label">
                        Budget Amount
                      </label>
                      <input
                        type="number"
                        className={`form-control ${errors.amount ? 'is-invalid' : ''}`}
                        id="amount"
                        name="amount"
                        value={budget.amount || ''}
                        onChange={handleInputChange}
                        step="0.01"
                        min="0"
                      />
                      {errors.amount && (
                        <div className="invalid-feedback">{errors.amount}</div>
                      )}
                    </div>
                  </div>
                )}
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onClose}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={isSaving || isLoading}>
                  {isSaving ? (
                    <>
                      <span
                        className="spinner-border spinner-border-sm"
                        role="status"
                        aria-hidden="true"
                      ></span>
                      <span className="ms-2">Saving...</span>
                    </>
                  ) : (
                    <span>Save Budget</span>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  );
};

export default BudgetForm;
