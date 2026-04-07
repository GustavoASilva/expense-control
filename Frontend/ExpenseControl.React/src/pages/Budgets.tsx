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

  const getStatusText = (percent: number): string => {
    if (percent >= 100) return 'Over budget';
    if (percent >= 80) return 'Almost spent';
    if (percent >= 50) return 'On track';
    return 'Under budget';
  };

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 5 }, (_, i) => currentYear - 2 + i);

  return (
    <div className="fade-in">
      <div className="page-header">
        <h1 className="page-title">Budgets</h1>
        <div className="d-flex gap-2 align-items-center flex-wrap">
          <select
            className="form-select"
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(parseInt(e.target.value))}
            style={{ width: 'auto' }}
          >
            {monthNames.map((name, index) => (
              <option key={index + 1} value={index + 1}>
                {name}
              </option>
            ))}
          </select>
          <select
            className="form-select"
            value={selectedYear}
            onChange={(e) => setSelectedYear(parseInt(e.target.value))}
            style={{ width: 'auto' }}
          >
            {years.map((year) => (
              <option key={year} value={year}>
                {year}
              </option>
            ))}
          </select>
          <button className="btn btn-primary" onClick={handleCreateBudget}>
            <i className="bi bi-plus-lg me-2"></i>New Budget
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="d-flex align-items-center justify-content-center" style={{ minHeight: '300px' }}>
          <div className="text-center">
            <div className="spinner-border text-primary mb-3" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
            <p className="text-muted mb-0">Loading budgets...</p>
          </div>
        </div>
      ) : budgets.length === 0 ? (
        <div className="card">
          <div className="card-body">
            <div className="empty-state">
              <div className="empty-state-icon">
                <i className="bi bi-pie-chart"></i>
              </div>
              <div className="empty-state-title">No budgets found</div>
              <div className="empty-state-description">
                Create a budget for {monthNames[selectedMonth - 1]} {selectedYear} to track your spending
              </div>
              <button className="btn btn-primary" onClick={handleCreateBudget}>
                <i className="bi bi-plus-lg me-2"></i>Create Budget
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="row g-4">
          {budgets.map((budget) => {
            const usage = budget.usage;
            const spent = usage?.usage || 0;
            const remaining = budget.amount - spent;
            const percent = usage?.percent || 0;
            const percentColor = getPercentageColor(percent);
            const statusText = getStatusText(percent);

            return (
              <div key={budget.id} className="col-md-6 col-lg-4">
                <div 
                  className="card h-100" 
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleEditBudget(budget)}
                >
                  <div className="card-body">
                    <div className="d-flex justify-content-between align-items-start mb-3">
                      <div>
                        <h5 className="mb-1">{budget.category?.name || 'Unknown'}</h5>
                        <span className={`badge bg-${percentColor}`} style={{ fontSize: '0.75rem' }}>
                          {statusText}
                        </span>
                      </div>
                      <button
                        className="btn btn-sm btn-outline-primary"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleEditBudget(budget);
                        }}
                      >
                        <i className="bi bi-pencil"></i>
                      </button>
                    </div>

                    <div className="mb-3">
                      <div className="d-flex justify-content-between mb-2">
                        <span className="text-muted" style={{ fontSize: '0.8125rem' }}>
                          {formatCurrency(spent)} spent
                        </span>
                        <span className="text-muted" style={{ fontSize: '0.8125rem' }}>
                          {formatCurrency(budget.amount)}
                        </span>
                      </div>
                      <div className="progress" style={{ height: '8px' }}>
                        <div
                          className={`progress-bar bg-${percentColor}`}
                          role="progressbar"
                          style={{ width: `${Math.min(percent, 100)}%` }}
                          aria-valuenow={percent}
                          aria-valuemin={0}
                          aria-valuemax={100}
                        />
                      </div>
                    </div>

                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <span className="text-muted" style={{ fontSize: '0.75rem' }}>Remaining</span>
                        <div className={`fw-semibold ${remaining < 0 ? 'text-danger' : 'text-success'}`}>
                          {formatCurrency(remaining)}
                        </div>
                      </div>
                      <div className="text-end">
                        <span className="text-muted" style={{ fontSize: '0.75rem' }}>Used</span>
                        <div className={`fw-semibold text-${percentColor}`}>
                          {percent.toFixed(0)}%
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
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

