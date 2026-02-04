import React, { useState, useEffect } from 'react';
import { getBudgets, getBudgetUsage } from '../services/api';
import { Budget, BudgetUsage } from '../types/index';
import BudgetForm from '../components/BudgetForm';

interface BudgetWithUsage extends Budget {
  usage?: BudgetUsage;
}

const Budgets: React.FC = () => {
  const now = new Date();
  const [isLoading, setIsLoading] = useState(true);
  const [budgets, setBudgets] = useState<BudgetWithUsage[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editBudget, setEditBudget] = useState<Budget | null>(null);
  const [selectedYear, setSelectedYear] = useState(now.getFullYear());
  const [selectedMonth, setSelectedMonth] = useState(now.getMonth() + 1);

  useEffect(() => {
    loadBudgets();
  }, [selectedYear, selectedMonth]);

  const loadBudgets = async () => {
    setIsLoading(true);
    try {
      const data = await getBudgets(selectedYear, selectedMonth);
      
      // Load usage for each budget with error handling
      const budgetsWithUsage = await Promise.all(
        data.map(async (budget) => {
          try {
            const usage = await getBudgetUsage(budget.categoryId, budget.year, budget.month);
            return { ...budget, usage };
          } catch (error) {
            // Log error but continue with other budgets
            console.error(`Failed to load usage for budget ${budget.id}:`, error);
            // Return budget without usage data
            return { ...budget, usage: undefined };
          }
        })
      );
      
      setBudgets(budgetsWithUsage);
    } catch (error) {
      console.error('Failed to load budgets:', error);
      // Set empty budgets on error so UI shows empty state
      setBudgets([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateBudget = () => {
    setEditBudget(null);
    setShowForm(true);
  };

  const handleEditBudget = (budget: Budget) => {
    setEditBudget(budget);
    setShowForm(true);
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditBudget(null);
  };

  const handleFormSave = () => {
    loadBudgets();
  };

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const getPercentageColor = (percent: number): string => {
    if (percent >= 100) return 'danger';
    if (percent >= 80) return 'warning';
    return 'success';
  };

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 5 }, (_, i) => currentYear - 2 + i);

  return (
    <div className="container-fluid py-3">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Budgets</h1>
        <div className="d-flex gap-2">
          <select
            className="form-select form-select-sm"
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(parseInt(e.target.value))}
          >
            {monthNames.map((name, index) => (
              <option key={index + 1} value={index + 1}>
                {name}
              </option>
            ))}
          </select>
          <select
            className="form-select form-select-sm"
            value={selectedYear}
            onChange={(e) => setSelectedYear(parseInt(e.target.value))}
          >
            {years.map((year) => (
              <option key={year} value={year}>
                {year}
              </option>
            ))}
          </select>
          <button className="btn btn-primary btn-sm" onClick={handleCreateBudget}>
            <i className="bi bi-plus"></i> New Budget
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : (
        <div className="card shadow-sm">
          <div className="card-body">
            {budgets.length === 0 ? (
              <div className="text-center py-5 text-muted">
                <i className="bi bi-wallet2 fs-1"></i>
                <p className="mt-2">No budgets found for {monthNames[selectedMonth - 1]} {selectedYear}</p>
                <button className="btn btn-primary" onClick={handleCreateBudget}>
                  Create your first budget
                </button>
              </div>
            ) : (
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>Category</th>
                      <th>Period</th>
                      <th className="text-end">Budget</th>
                      <th className="text-end">Spent</th>
                      <th className="text-end">Remaining</th>
                      <th style={{ width: '30%' }}>Usage</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {budgets.map((budget) => {
                      const usage = budget.usage;
                      const spent = usage?.usage || 0;
                      const remaining = budget.amount - spent;
                      const percent = usage?.percent || 0;
                      const percentColor = getPercentageColor(percent);

                      return (
                        <tr key={budget.id}>
                          <td>{budget.category?.name || 'Unknown'}</td>
                          <td>
                            {monthNames[budget.month - 1]} {budget.year}
                          </td>
                          <td className="text-end">{formatCurrency(budget.amount)}</td>
                          <td className="text-end">{formatCurrency(spent)}</td>
                          <td className="text-end">
                            <span className={`text-${remaining < 0 ? 'danger' : 'success'}`}>
                              {formatCurrency(remaining)}
                            </span>
                          </td>
                          <td>
                            <div className="d-flex align-items-center gap-2">
                              <div className="progress flex-grow-1" style={{ height: '20px' }}>
                                <div
                                  className={`progress-bar bg-${percentColor}`}
                                  role="progressbar"
                                  style={{ width: `${Math.min(percent, 100)}%` }}
                                  aria-valuenow={percent}
                                  aria-valuemin={0}
                                  aria-valuemax={100}
                                >
                                  {percent.toFixed(0)}%
                                </div>
                              </div>
                            </div>
                          </td>
                          <td className="text-end">
                            <button
                              className="btn btn-outline-primary btn-sm"
                              onClick={() => handleEditBudget(budget)}
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      <BudgetForm
        show={showForm}
        onClose={handleFormClose}
        onSave={handleFormSave}
        editBudget={editBudget}
      />
    </div>
  );
};

export default Budgets;

