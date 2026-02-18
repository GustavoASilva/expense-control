import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js';
import { Bar } from 'react-chartjs-2';
import DateRangePicker from '../components/DateRangePicker';
import StatCard from '../components/StatCard';
import {
  getBalance,
  getBalanceByCategory,
  getMonthlyBalance,
  getTransactions,
  getCategories,
} from '../services/api';
import {
  Balance,
  CategoryBalance,
  MonthlyBalance,
  Transaction,
  Category,
  TransactionType,
} from '../types/index';

// Register Chart.js components
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const Dashboard: React.FC = () => {
  const [isLoading, setIsLoading] = useState(true);
  const today = new Date();
  const defaultStart = new Date(today);
  defaultStart.setDate(today.getDate() - 29);

  const [startDate, setStartDate] = useState(defaultStart.toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(today.toISOString().split('T')[0]);
  const [balance, setBalance] = useState<Balance | null>(null);
  const [previousBalance, setPreviousBalance] = useState<Balance | null>(null);
  const [categoryBalances, setCategoryBalances] = useState<CategoryBalance[]>([]);
  const [monthlyBalance, setMonthlyBalance] = useState<MonthlyBalance | null>(null);
  const [recentTransactions, setRecentTransactions] = useState<Transaction[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);

  useEffect(() => {
    loadCategories();
  }, []);

  useEffect(() => {
    loadData(startDate, endDate);
  }, [startDate, endDate]);

  const loadCategories = async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch (error) {
      console.error('Failed to load categories:', error);
    }
  };

  const loadData = async (start: string, end: string) => {
    setIsLoading(true);
    try {
      // Load current period data
      const [balanceData, categoryBalanceData, monthlyBalanceData, transactionsData] = await Promise.all([
        getBalance(start, end),
        getBalanceByCategory(start, end),
        getMonthlyBalance(new Date().getFullYear()),
        getTransactions(start, end),
      ]);

      // Calculate previous period dates
      const startDateObj = new Date(start);
      const endDateObj = new Date(end);
      const periodLength = Math.floor((endDateObj.getTime() - startDateObj.getTime()) / (1000 * 60 * 60 * 24)) + 1;
      const previousStart = new Date(startDateObj);
      previousStart.setDate(startDateObj.getDate() - periodLength);
      const previousEnd = new Date(startDateObj);
      previousEnd.setDate(startDateObj.getDate() - 1);

      const previousBalanceData = await getBalance(
        previousStart.toISOString().split('T')[0],
        previousEnd.toISOString().split('T')[0]
      );

      setBalance(balanceData);
      setCategoryBalances(categoryBalanceData);
      setMonthlyBalance(monthlyBalanceData);
      setRecentTransactions(
        transactionsData
          .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
          .slice(0, 5)
      );
      setPreviousBalance(previousBalanceData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDateRangeChanged = (range: { start: string; end: string }) => {
    setStartDate(range.start);
    setEndDate(range.end);
  };

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
    });
  };

  const chartData = monthlyBalance && monthlyBalance.hasTransactions
    ? {
        labels: monthlyBalance.months.map((m) => m.monthName.substring(0, 3)),
        datasets: [
          {
            label: 'Income',
            backgroundColor: 'rgba(72, 187, 120, 0.8)',
            borderColor: 'rgba(72, 187, 120, 1)',
            borderWidth: 0,
            borderRadius: 4,
            data: monthlyBalance.months.map((m) => m.income),
          },
          {
            label: 'Expenses',
            backgroundColor: 'rgba(245, 101, 101, 0.8)',
            borderColor: 'rgba(245, 101, 101, 1)',
            borderWidth: 0,
            borderRadius: 4,
            data: monthlyBalance.months.map((m) => m.expenses),
          },
        ],
      }
    : null;

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const,
        labels: {
          usePointStyle: true,
          padding: 20,
          font: {
            size: 12,
            weight: 500 as const,
          },
        },
      },
      title: { display: false },
    },
    scales: {
      x: {
        grid: {
          display: false,
        },
        ticks: {
          font: {
            size: 11,
          },
        },
      },
      y: {
        beginAtZero: true,
        grid: {
          color: 'rgba(0, 0, 0, 0.05)',
        },
        ticks: {
          font: {
            size: 11,
          },
        },
      },
    },
  };

  if (isLoading) {
    return (
      <div className="d-flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
        <div className="text-center">
          <div className="spinner-border text-primary mb-3" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="text-muted mb-0">Loading dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="page-header">
        <h1 className="page-title">Dashboard</h1>
        <DateRangePicker
          startDate={startDate}
          endDate={endDate}
          onChanged={handleDateRangeChanged}
        />
      </div>

      <div className="stat-card-grid">
        <StatCard
          title="Income"
          value={balance?.income || 0}
          icon="bi-arrow-down-circle"
          type="income"
          previousValue={previousBalance?.income}
        />
        <StatCard
          title="Expenses"
          value={balance?.expenses || 0}
          icon="bi-arrow-up-circle"
          type="expense"
          previousValue={previousBalance?.expenses}
        />
        <StatCard
          title="Balance"
          value={balance?.balance || 0}
          icon="bi-wallet2"
          type="balance"
          previousValue={
            previousBalance ? previousBalance.income - previousBalance.expenses : undefined
          }
        />
      </div>

      <div className="row g-4">
        <div className="col-lg-8">
          <div className="card">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-center mb-4">
                <h5 className="card-title mb-0">
                  Monthly Overview {monthlyBalance?.year || new Date().getFullYear()}
                </h5>
              </div>
              {!monthlyBalance?.hasTransactions ? (
                <div className="empty-state">
                  <div className="empty-state-icon">
                    <i className="bi bi-bar-chart-line"></i>
                  </div>
                  <div className="empty-state-title">No data yet</div>
                  <div className="empty-state-description">
                    Start adding transactions to see your monthly overview
                  </div>
                </div>
              ) : (
                <div className="chart-container">
                  {chartData && <Bar data={chartData} options={chartOptions} />}
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="card h-100">
            <div className="card-body">
              <h5 className="card-title">Top Categories</h5>
              {categoryBalances.length === 0 ? (
                <div className="empty-state py-4">
                  <div className="empty-state-icon" style={{ width: '60px', height: '60px', fontSize: '1.5rem' }}>
                    <i className="bi bi-pie-chart"></i>
                  </div>
                  <div className="empty-state-description mb-0">
                    No category data available
                  </div>
                </div>
              ) : (
                <div className="d-flex flex-column gap-3">
                  {categoryBalances.slice(0, 5).map((category) => {
                    const cat = categories.find((c) => c.name === category.categoryName);
                    const isIncome = category.transactionType === TransactionType.Income;
                    return (
                      <div key={category.categoryName} className="d-flex justify-content-between align-items-center">
                        <div className="d-flex align-items-center gap-3">
                          <div
                            className={`transaction-icon ${isIncome ? 'income' : 'expense'}`}
                            style={{ width: '36px', height: '36px', fontSize: '0.875rem' }}
                          >
                            <i className={`bi bi-${cat?.iconName || 'tag'}`}></i>
                          </div>
                          <div>
                            <div className="fw-medium">{category.categoryName}</div>
                            <div className="text-muted" style={{ fontSize: '0.75rem' }}>
                              {category.count} transactions
                            </div>
                          </div>
                        </div>
                        <div className={`fw-semibold ${isIncome ? 'text-success' : 'text-danger'}`}>
                          {isIncome ? '+' : '-'}{formatCurrency(category.total)}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-body">
          <div className="d-flex justify-content-between align-items-center mb-4">
            <h5 className="card-title mb-0">Recent Transactions</h5>
            <Link to="/transactions" className="btn btn-outline-primary btn-sm">
              View All <i className="bi bi-arrow-right ms-1"></i>
            </Link>
          </div>
          {recentTransactions.length === 0 ? (
            <div className="empty-state">
              <div className="empty-state-icon">
                <i className="bi bi-receipt"></i>
              </div>
              <div className="empty-state-title">No transactions yet</div>
              <div className="empty-state-description">
                Add your first transaction to get started
              </div>
              <Link to="/transactions/new" className="btn btn-primary">
                <i className="bi bi-plus me-2"></i>Add Transaction
              </Link>
            </div>
          ) : (
            <div>
              {recentTransactions.map((transaction) => {
                const isIncome = transaction.type === TransactionType.Income;
                return (
                  <div key={transaction.id} className="transaction-row">
                    <div className={`transaction-icon ${isIncome ? 'income' : 'expense'}`}>
                      <i className={`bi ${isIncome ? 'bi-arrow-down-left' : 'bi-arrow-up-right'}`}></i>
                    </div>
                    <div className="transaction-details">
                      <div className="transaction-description">{transaction.description}</div>
                      <div className="transaction-meta">
                        <span className="category-badge me-2" style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem' }}>
                          {transaction.category?.name || transaction.categoryName}
                        </span>
                        {formatDate(transaction.date)}
                      </div>
                    </div>
                    <div className={`transaction-amount ${isIncome ? 'income' : 'expense'}`}>
                      {isIncome ? '+' : '-'}{formatCurrency(transaction.amount)}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
