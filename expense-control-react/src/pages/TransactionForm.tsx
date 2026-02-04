import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { getCategories, getTransaction, createTransaction, updateTransaction } from '../services/api';
import { Category, TransactionForm as TransactionFormType, TransactionType } from '../types/index';

const TransactionForm: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isNew = !id;

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [transaction, setTransaction] = useState<TransactionFormType>({
    amount: 0,
    description: '',
    date: new Date().toISOString().split('T')[0],
    type: TransactionType.Expense,
    categoryId: '',
    notes: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    loadData();
  }, [id]);

  const loadData = async () => {
    setIsLoading(true);
    try {
      const categoriesData = await getCategories();
      setCategories(categoriesData);

      if (!isNew && id) {
        const transactionData = await getTransaction(id);
        setTransaction({
          id: transactionData.id,
          amount: transactionData.amount,
          description: transactionData.description,
          date: transactionData.date,
          type: transactionData.type,
          categoryId: transactionData.categoryId,
          notes: '',
        });
      }
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setTransaction((prev) => ({
      ...prev,
      [name]: name === 'amount' ? parseFloat(value) || 0 : value,
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

  const handleTypeChange = (type: TransactionType) => {
    setTransaction((prev) => ({
      ...prev,
      type,
      categoryId: '', // Reset category when type changes
    }));
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (transaction.amount <= 0) {
      newErrors.amount = 'Amount must be greater than 0';
    }

    if (!transaction.description || transaction.description.length < 3) {
      newErrors.description = 'Description must be at least 3 characters';
    }

    if (transaction.description.length > 200) {
      newErrors.description = 'Description must be less than 200 characters';
    }

    if (!transaction.date) {
      newErrors.date = 'Date is required';
    }

    if (!transaction.categoryId) {
      newErrors.categoryId = 'Please select a category';
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
      if (isNew) {
        await createTransaction(transaction);
      } else if (id) {
        await updateTransaction(id, transaction);
      }
      navigate('/transactions');
    } catch (error) {
      console.error('Failed to save transaction:', error);
      alert('Failed to save transaction. Please try again.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    navigate('/transactions');
  };

  const filteredCategories = categories.filter((c) => c.type === transaction.type);

  if (isLoading) {
    return (
      <div className="d-flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
        <div className="text-center">
          <div className="spinner-border text-primary mb-3" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="text-muted mb-0">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="mb-4">
        <Link to="/transactions" className="text-muted text-decoration-none d-inline-flex align-items-center mb-3">
          <i className="bi bi-arrow-left me-2"></i>Back to Transactions
        </Link>
        <h1 className="page-title">{isNew ? 'New Transaction' : 'Edit Transaction'}</h1>
      </div>

      <div className="row justify-content-center">
        <div className="col-lg-8 col-xl-6">
          <form onSubmit={handleSubmit}>
            <div className="card">
              <div className="card-body">
                <div className="mb-4">
                  <label className="form-label">Transaction Type</label>
                  <div className="type-toggle">
                    <input
                      type="radio"
                      className="btn-check"
                      name="type"
                      id="expense"
                      checked={transaction.type === TransactionType.Expense}
                      onChange={() => handleTypeChange(TransactionType.Expense)}
                    />
                    <label className="btn btn-outline-danger" htmlFor="expense">
                      <i className="bi bi-arrow-up-right me-2"></i>Expense
                    </label>

                    <input
                      type="radio"
                      className="btn-check"
                      name="type"
                      id="income"
                      checked={transaction.type === TransactionType.Income}
                      onChange={() => handleTypeChange(TransactionType.Income)}
                    />
                    <label className="btn btn-outline-success" htmlFor="income">
                      <i className="bi bi-arrow-down-left me-2"></i>Income
                    </label>
                  </div>
                </div>

                <div className="row g-4">
                  <div className="col-md-6">
                    <label htmlFor="amount" className="form-label">
                      Amount
                    </label>
                    <div className="input-group">
                      <span className="input-group-text">$</span>
                      <input
                        type="number"
                        className={`form-control ${errors.amount ? 'is-invalid' : ''}`}
                        id="amount"
                        name="amount"
                        value={transaction.amount || ''}
                        onChange={handleInputChange}
                        step="0.01"
                        min="0"
                        placeholder="0.00"
                      />
                      {errors.amount && <div className="invalid-feedback">{errors.amount}</div>}
                    </div>
                  </div>

                  <div className="col-md-6">
                    <label htmlFor="date" className="form-label">
                      Date
                    </label>
                    <input
                      type="date"
                      className={`form-control ${errors.date ? 'is-invalid' : ''}`}
                      id="date"
                      name="date"
                      value={transaction.date}
                      onChange={handleInputChange}
                    />
                    {errors.date && <div className="invalid-feedback">{errors.date}</div>}
                  </div>

                  <div className="col-12">
                    <label htmlFor="description" className="form-label">
                      Description
                    </label>
                    <input
                      type="text"
                      className={`form-control ${errors.description ? 'is-invalid' : ''}`}
                      id="description"
                      name="description"
                      value={transaction.description}
                      onChange={handleInputChange}
                      placeholder="What was this transaction for?"
                    />
                    {errors.description && (
                      <div className="invalid-feedback">{errors.description}</div>
                    )}
                  </div>

                  <div className="col-12">
                    <label htmlFor="categoryId" className="form-label">
                      Category
                    </label>
                    <select
                      className={`form-select ${errors.categoryId ? 'is-invalid' : ''}`}
                      id="categoryId"
                      name="categoryId"
                      value={transaction.categoryId}
                      onChange={handleInputChange}
                    >
                      <option value="">Select a category</option>
                      {filteredCategories.map((category) => (
                        <option key={category.id} value={category.id}>
                          {category.name}
                        </option>
                      ))}
                    </select>
                    {errors.categoryId && (
                      <div className="invalid-feedback">{errors.categoryId}</div>
                    )}
                  </div>

                  <div className="col-12">
                    <label htmlFor="notes" className="form-label">
                      Notes <span className="text-muted">(optional)</span>
                    </label>
                    <textarea
                      className="form-control"
                      id="notes"
                      name="notes"
                      value={transaction.notes || ''}
                      onChange={handleInputChange}
                      rows={3}
                      placeholder="Add any additional notes..."
                    />
                  </div>
                </div>
              </div>
              <div className="card-footer d-flex justify-content-between">
                <button type="button" className="btn btn-outline-secondary" onClick={handleCancel}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={isSaving}>
                  {isSaving ? (
                    <>
                      <span
                        className="spinner-border spinner-border-sm me-2"
                        role="status"
                        aria-hidden="true"
                      ></span>
                      Saving...
                    </>
                  ) : (
                    <>
                      <i className="bi bi-check-lg me-2"></i>
                      {isNew ? 'Create Transaction' : 'Save Changes'}
                    </>
                  )}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default TransactionForm;
