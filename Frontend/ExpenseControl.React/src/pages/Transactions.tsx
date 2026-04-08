import React, { useState, useEffect } from 'react';
import DateRangePicker from '../components/DateRangePicker';
import TransactionForm from '../components/TransactionForm';
import { getTransactions, deleteTransaction } from '../services/api';
import { Transaction, TransactionType } from '../types/index';
import { useToast } from '../hooks/useToast';
import { useConfirm } from '../hooks/useConfirm';
import { shouldShowHeaderCreateButton } from '../utils/listUiState';

const Transactions: React.FC = () => {
  const { showToast } = useToast();
  const { confirm } = useConfirm();
  const [isLoading, setIsLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [recordForEdit, setRecordForEdit] = useState<Transaction | null>(null);
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

  const openFormForNew = () => {
    setRecordForEdit(null);
    setDialogOpen(true);
  };

  const openFormForEdit = (txn: Transaction) => {
    setRecordForEdit(txn);
    setDialogOpen(true);
  };

  const hideForm = () => {
    setDialogOpen(false);
    setRecordForEdit(null);
  };

  const reloadAfterSave = async () => {
    await loadData(startDate, endDate);
  };

  const handleDeleteTransaction = async (transaction: Transaction) => {
    const confirmed = await confirm({
      title: 'Delete Transaction',
      message: `Are you sure you want to delete "${transaction.description}" (${formatCurrency(transaction.amount)})?`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      variant: 'danger',
    });

    if (confirmed) {
      try {
        await deleteTransaction(transaction.id);
        await loadData(startDate, endDate);
      } catch (error) {
        console.error('Failed to delete transaction:', error);
        showToast('Failed to delete transaction. Please try again.', 'danger');
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
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
    });
  };

  // Group transactions by date
  const groupedTransactions = transactions.reduce((groups, transaction) => {
    const date = transaction.date.split('T')[0];
    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(transaction);
    return groups;
  }, {} as Record<string, Transaction[]>);

  return (
    <div className="fade-in">
      <div className="page-header">
        <h1 className="page-title">Transactions</h1>
        <div className="d-flex gap-3 align-items-center flex-wrap">
          <DateRangePicker
            startDate={startDate}
            endDate={endDate}
            onChanged={handleDateRangeChanged}
          />
          {shouldShowHeaderCreateButton(isLoading, transactions.length) && (
            <button className="btn btn-primary" onClick={openFormForNew}>
              <i className="bi bi-plus-lg me-2"></i>New Transaction
            </button>
          )}
        </div>
      </div>

      {isLoading ? (
        <div className="d-flex align-items-center justify-content-center" style={{ minHeight: '300px' }}>
          <div className="text-center">
            <div className="spinner-border text-primary mb-3" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
            <p className="text-muted mb-0">Loading transactions...</p>
          </div>
        </div>
      ) : transactions.length === 0 ? (
        <div className="card">
          <div className="card-body">
            <div className="empty-state">
              <div className="empty-state-icon">
                <i className="bi bi-receipt"></i>
              </div>
              <div className="empty-state-title">No transactions found</div>
              <div className="empty-state-description">
                Get started by creating your first transaction
              </div>
              <button className="btn btn-primary" onClick={openFormForNew}>
                <i className="bi bi-plus-lg me-2"></i>Create Transaction
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="card">
          <div className="card-body p-0">
            {Object.entries(groupedTransactions).map(([date, dayTransactions]) => (
              <div key={date}>
                <div
                  className="px-4 py-2 bg-light border-bottom"
                  style={{ fontSize: '0.8125rem', fontWeight: 600, color: 'var(--text-secondary)' }}
                >
                  {formatDate(date)}
                </div>
                {dayTransactions.map((transaction) => {
                  const isIncome = transaction.type === TransactionType.Income;
                  return (
                    <div
                      key={transaction.id}
                      className="transaction-row"
                      style={{ cursor: 'pointer' }}
                      onClick={() => openFormForEdit(transaction)}
                    >
                      <div className={`transaction-icon ${isIncome ? 'income' : 'expense'}`}>
                        <i className={`bi ${isIncome ? 'bi-arrow-down-left' : 'bi-arrow-up-right'}`}></i>
                      </div>
                      <div className="transaction-details">
                        <div className="transaction-description">{transaction.description}</div>
                        <div className="transaction-meta">
                          <span className="category-badge" style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem' }}>
                            {transaction.category?.name || transaction.categoryName}
                          </span>
                          {transaction.notes && (
                            <span className="ms-2" title={transaction.notes} style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                              <i className="bi bi-chat-dots"></i> Note
                            </span>
                          )}
                        </div>
                      </div>
                      <div className={`transaction-amount ${isIncome ? 'income' : 'expense'}`}>
                        {isIncome ? '+' : '-'}{formatCurrency(transaction.amount)}
                      </div>
                      <div className="ms-3 d-flex gap-1" onClick={(e) => e.stopPropagation()}>
                        <button
                          className="btn btn-sm btn-outline-primary"
                          style={{ padding: '0.25rem 0.5rem' }}
                          onClick={() => openFormForEdit(transaction)}
                          title="Edit"
                        >
                          <i className="bi bi-pencil"></i>
                        </button>
                        <button
                          className="btn btn-sm btn-outline-danger"
                          style={{ padding: '0.25rem 0.5rem' }}
                          onClick={() => handleDeleteTransaction(transaction)}
                          title="Delete"
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            ))}
          </div>
        </div>
      )}

      <TransactionForm
        isVisible={dialogOpen}
        handleClose={hideForm}
        refreshData={reloadAfterSave}
        recordToUpdate={recordForEdit}
      />
    </div>
  );
};

export default Transactions;
