import React, { useState, useEffect, useRef } from 'react';
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
    return new Date(dateString).toLocaleDateString();
  };

  const chartData = monthlyBalance && monthlyBalance.hasTransactions
    ? {
        labels: monthlyBalance.months.map((m) => m.monthName),
        datasets: [
          {
            label: 'Income',
            backgroundColor: 'rgba(40, 167, 69, 0.7)',
            borderColor: 'rgba(40, 167, 69, 1)',
            borderWidth: 1,
            data: monthlyBalance.months.map((m) => m.income),
          },
          {
            label: 'Expenses',
            backgroundColor: 'rgba(220, 53, 69, 0.7)',
            borderColor: 'rgba(220, 53, 69, 1)',
            borderWidth: 1,
            data: monthlyBalance.months.map((m) => m.expenses),
          },
        ],
      }
    : null;

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'top' as const },
      title: { display: true, text: 'Monthly Income vs Expenses' },
    },
    scales: {
      y: { beginAtZero: true },
    },
  };

  if (isLoading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid py-3">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Dashboard</h1>
        <DateRangePicker
          startDate={startDate}
          endDate={endDate}
          onChanged={handleDateRangeChanged}
        />
      </div>

      <div className="row g-4">
        <div className="col-md-4">
          <StatCard
            title="Income"
            value={balance?.income || 0}
            icon="bi-graph-up-arrow"
            previousValue={previousBalance?.income}
          />
        </div>
        <div className="col-md-4">
          <StatCard
            title="Expenses"
            value={balance?.expenses || 0}
            icon="bi-graph-down-arrow"
            previousValue={previousBalance?.expenses}
          />
        </div>
        <div className="col-md-4">
          <StatCard
            title="Balance"
            value={balance?.currentBalance || 0}
            icon="bi-wallet2"
            inverted={true}
            previousValue={
              previousBalance ? previousBalance.income - previousBalance.expenses : undefined
            }
          />
        </div>
      </div>

      <div className="row mt-4">
        <div className="col-lg-8">
          <div className="card shadow-sm">
            <div className="card-body">
              <h5 className="card-title">
                Monthly Overview ({monthlyBalance?.year || new Date().getFullYear()})
              </h5>
              {!monthlyBalance?.hasTransactions ? (
                <div className="text-center py-5 text-muted">
                  <i className="bi bi-bar-chart-line fs-1"></i>
                  <p className="mt-2">No transactions recorded yet</p>
                </div>
              ) : (
                <div className="chart-container" style={{ position: 'relative', height: '300px' }}>
                  {chartData && <Bar data={chartData} options={chartOptions} />}
                </div>
              )}
            </div>
          </div>
        </div>
        <div className="col-lg-4">
          <div className="card shadow-sm">
            <div className="card-body">
              <h5 className="card-title">Top Categories</h5>
              {categoryBalances.length === 0 ? (
                <div className="text-center py-5 text-muted">
                  <i className="bi bi-pie-chart fs-1"></i>
                  <p className="mt-2">No category data available</p>
                </div>
              ) : (
                <div className="list-group list-group-flush">
                  {categoryBalances.slice(0, 5).map((category) => {
                    const cat = categories.find((c) => c.name === category.categoryName);
                    return (
                      <div key={category.categoryName} className="list-group-item border-0 px-0">
                        <div className="d-flex justify-content-between align-items-center">
                          <div>
                            {cat && <i className={`bi bi-${cat.iconName} me-2`}></i>}
                            <h6 className="mb-0 d-inline">{category.categoryName}</h6>
                            <br />
                            <small className="text-muted">{category.count} transactions</small>
                          </div>
                          <span
                            className={
                              category.transactionType === TransactionType.Income
                                ? 'text-success'
                                : 'text-danger'
                            }
                          >
                            {category.transactionType === TransactionType.Income ? '+' : '-'}
                            {formatCurrency(category.total)}
                          </span>
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

      <div className="card shadow-sm mt-4">
        <div className="card-body">
          <div className="d-flex justify-content-between align-items-center mb-4">
            <h5 className="card-title mb-0">Recent Transactions</h5>
            <Link to="/transactions" className="btn btn-primary btn-sm">
              View All
            </Link>
          </div>
          {recentTransactions.length === 0 ? (
            <div className="text-center py-5 text-muted">
              <i className="bi bi-receipt fs-1"></i>
              <p className="mt-2">No transactions found</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Description</th>
                    <th>Category</th>
                    <th className="text-end">Amount</th>
                  </tr>
                </thead>
                <tbody>
                  {recentTransactions.map((transaction) => (
                    <tr key={transaction.id}>
                      <td>{formatDate(transaction.date)}</td>
                      <td>{transaction.description}</td>
                      <td>{transaction.categoryName}</td>
                      <td className="text-end">
                        <span
                          className={
                            transaction.type === TransactionType.Income
                              ? 'text-success'
                              : 'text-danger'
                          }
                        >
                          {transaction.type === TransactionType.Income ? '+' : '-'}
                          {formatCurrency(transaction.amount)}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
