import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import DateRangePicker from '../components/DateRangePicker';
import { getTransactions, deleteTransaction } from '../services/api';
import { Transaction, TransactionType } from '../types/index';

const Transactions: React.FC = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(true);
  const today = new Date();
  const defaultStart = new Date(today);
  defaultStart.setDate(today.getDate() - 29);

  const [startDate, setStartDate] = useState(defaultStart.toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(today.toISOString().split('T')[0]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);

  useEffect(() => {
    loadData(startDate, endDate);
  }, [startDate, endDate]);

  const loadData = async (start: string, end: string) => {
    setIsLoading(true);
    try {
      const data = await getTransactions(start, end);
      setTransactions(
        data.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
      );
    } catch (error) {
      console.error('Failed to load transactions:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDateRangeChanged = (range: { start: string; end: string }) => {
    setStartDate(range.start);
    setEndDate(range.end);
  };

  const handleCreateTransaction = () => {
    navigate('/transactions/new');
  };

  const handleEditTransaction = (id: string) => {
    navigate(`/transactions/${id}`);
  };

  const handleDeleteTransaction = async (transaction: Transaction) => {
    if (
      window.confirm(
        `Are you sure you want to delete this transaction?\n\n${transaction.description}\n${formatCurrency(transaction.amount)}`
      )
    ) {
      try {
        await deleteTransaction(transaction.id);
        await loadData(startDate, endDate);
      } catch (error) {
        console.error('Failed to delete transaction:', error);
        alert('Failed to delete transaction. Please try again.');
      }
    }
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

  return (
    <div className="container-fluid py-3">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Transactions</h1>
        <div className="d-flex gap-2">
          <DateRangePicker
            small={true}
            startDate={startDate}
            endDate={endDate}
            onChanged={handleDateRangeChanged}
          />
          <button className="btn btn-primary btn-sm" onClick={handleCreateTransaction}>
            <i className="bi bi-plus"></i> New Transaction
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
            {transactions.length === 0 ? (
              <div className="text-center py-5 text-muted">
                <i className="bi bi-receipt fs-1"></i>
                <p className="mt-2">No transactions found</p>
                <button className="btn btn-primary" onClick={handleCreateTransaction}>
                  Create your first transaction
                </button>
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
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {transactions.map((transaction) => (
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
                        <td className="text-end">
                          <div className="btn-group btn-group-sm">
                            <button
                              className="btn btn-outline-primary"
                              onClick={() => handleEditTransaction(transaction.id)}
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button
                              className="btn btn-outline-danger"
                              onClick={() => handleDeleteTransaction(transaction)}
                            >
                              <i className="bi bi-trash"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default Transactions;
