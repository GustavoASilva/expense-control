import React, { useState, useEffect, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { getCategories, createCategory } from '../services/api';
import { Category, TransactionType, CategoryForm } from '../types/index';
import { useToast } from '../hooks/useToast';
import { useBodyScrollLock } from '../hooks/useBodyScrollLock';

const Categories: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const { showToast } = useToast();

  const loadCategories = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await getCategories();
      setCategories(data);
    } catch {
      showToast('Failed to load categories.', 'danger');
    } finally {
      setIsLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  const expenseCategories = categories.filter(c => c.type === TransactionType.Expense);
  const incomeCategories = categories.filter(c => c.type === TransactionType.Income);

  return (
    <div className="fade-in">
      <div className="page-header">
        <div className="d-flex align-items-center gap-3">
          <i className="bi bi-tags fs-4 text-primary"></i>
          <div>
            <h1 className="page-title mb-0">Categories</h1>
            <p className="text-muted small mb-0">Manage transaction categories</p>
          </div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(true)}>
          <i className="bi bi-plus-lg me-2"></i>
          New Category
        </button>
      </div>

      {isLoading ? (
        <div className="d-flex justify-content-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : (
        <div className="row g-4">
          <div className="col-lg-6">
            <div className="card">
              <div className="card-header">
                <h5 className="mb-0">
                  <i className="bi bi-arrow-up-right text-danger me-2"></i>
                  Expense Categories
                </h5>
              </div>
              <div className="list-group list-group-flush">
                {expenseCategories.length === 0 ? (
                  <div className="list-group-item text-muted text-center py-4">
                    No expense categories yet.
                  </div>
                ) : (
                  expenseCategories.map(cat => (
                    <div key={cat.id} className="list-group-item d-flex align-items-center">
                      <i className={`bi bi-${cat.iconName} me-3 fs-5 text-danger`}></i>
                      <div className="flex-grow-1">
                        <div className="fw-medium">{cat.name}</div>
                        {cat.description && (
                          <small className="text-muted">{cat.description}</small>
                        )}
                      </div>
                      {cat.isDefault && (
                        <span className="badge bg-secondary-subtle text-secondary">Default</span>
                      )}
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>

          <div className="col-lg-6">
            <div className="card">
              <div className="card-header">
                <h5 className="mb-0">
                  <i className="bi bi-arrow-down-left text-success me-2"></i>
                  Income Categories
                </h5>
              </div>
              <div className="list-group list-group-flush">
                {incomeCategories.length === 0 ? (
                  <div className="list-group-item text-muted text-center py-4">
                    No income categories yet.
                  </div>
                ) : (
                  incomeCategories.map(cat => (
                    <div key={cat.id} className="list-group-item d-flex align-items-center">
                      <i className={`bi bi-${cat.iconName} me-3 fs-5 text-success`}></i>
                      <div className="flex-grow-1">
                        <div className="fw-medium">{cat.name}</div>
                        {cat.description && (
                          <small className="text-muted">{cat.description}</small>
                        )}
                      </div>
                      {cat.isDefault && (
                        <span className="badge bg-secondary-subtle text-secondary">Default</span>
                      )}
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      <CategoryFormModal
        show={showForm}
        onClose={() => setShowForm(false)}
        onSave={loadCategories}
      />
    </div>
  );
};

interface CategoryFormModalProps {
  show: boolean;
  onClose: () => void;
  onSave: () => void;
}

const CategoryFormModal: React.FC<CategoryFormModalProps> = ({ show, onClose, onSave }) => {
  const { showToast } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [form, setForm] = useState<CategoryForm>({
    name: '',
    description: '',
    type: TransactionType.Expense,
    iconName: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (show) {
      setForm({
        name: '',
        description: '',
        type: TransactionType.Expense,
        iconName: '',
      });
      setErrors({});
    }
  }, [show]);

  useBodyScrollLock(show);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};
    const trimmedName = form.name.trim();

    if (!trimmedName) {
      newErrors.name = 'Category name is required.';
    } else if (trimmedName.length < 2) {
      newErrors.name = 'Name must be at least 2 characters.';
    } else if (trimmedName.length > 100) {
      newErrors.name = 'Name must be at most 100 characters.';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate() || isSubmitting) return;

    setIsSubmitting(true);
    try {
      await createCategory({
        name: form.name.trim(),
        description: form.description?.trim() || undefined,
        type: form.type,
        iconName: form.iconName?.trim() || undefined,
      });
      showToast('Category created successfully!', 'success');
      onSave();
      onClose();
    } catch {
      showToast('Failed to create category. It may already exist.', 'danger');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleInputChange = (field: keyof CategoryForm, value: string | TransactionType) => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  if (!show) return null;

  return createPortal(
    <div
      className="modal show d-block form-modal"
      tabIndex={-1}
      style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
      onClick={onClose}
    >
      <div className="modal-dialog modal-dialog-centered modal-dialog-scrollable" onClick={(e) => e.stopPropagation()}>
        <div className="modal-content">
          <form onSubmit={handleSubmit}>
            <div className="modal-header">
              <h5 className="modal-title">
                <i className="bi bi-tag me-2"></i>
                New Category
              </h5>
              <button type="button" className="btn-close" onClick={onClose} aria-label="Close"></button>
            </div>
            <div className="modal-body">
              <div className="row g-3">
                <div className="col-12">
                  <label className="form-label">Category Type</label>
                  <div className="type-toggle">
                    <input
                      type="radio"
                      className="btn-check"
                      name="categoryType"
                      id="expenseCatType"
                      checked={form.type === TransactionType.Expense}
                      onChange={() => handleInputChange('type', TransactionType.Expense)}
                    />
                    <label className="btn btn-outline-danger" htmlFor="expenseCatType">
                      <i className="bi bi-arrow-up-right me-2"></i>Expense
                    </label>

                    <input
                      type="radio"
                      className="btn-check"
                      name="categoryType"
                      id="incomeCatType"
                      checked={form.type === TransactionType.Income}
                      onChange={() => handleInputChange('type', TransactionType.Income)}
                    />
                    <label className="btn btn-outline-success" htmlFor="incomeCatType">
                      <i className="bi bi-arrow-down-left me-2"></i>Income
                    </label>
                  </div>
                </div>

                <div className="col-12">
                  <label htmlFor="categoryName" className="form-label">Name</label>
                  <input
                    type="text"
                    className={`form-control ${errors.name ? 'is-invalid' : ''}`}
                    id="categoryName"
                    value={form.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                    placeholder="e.g., Entertainment"
                    maxLength={100}
                    autoFocus
                  />
                  {errors.name && (
                    <div className="invalid-feedback">{errors.name}</div>
                  )}
                </div>

                <div className="col-12">
                  <label htmlFor="categoryDescription" className="form-label">
                    Description <span className="text-muted">(optional)</span>
                  </label>
                  <input
                    type="text"
                    className="form-control"
                    id="categoryDescription"
                    value={form.description || ''}
                    onChange={(e) => handleInputChange('description', e.target.value)}
                    placeholder="e.g., Movies, games, etc."
                    maxLength={500}
                  />
                </div>

                <div className="col-12">
                  <label htmlFor="categoryIcon" className="form-label">
                    Icon Name <span className="text-muted">(optional, Bootstrap Icons)</span>
                  </label>
                  <div className="input-group">
                    <span className="input-group-text">
                      <i className={`bi bi-${form.iconName?.trim() || 'tag'}`}></i>
                    </span>
                    <input
                      type="text"
                      className="form-control"
                      id="categoryIcon"
                      value={form.iconName || ''}
                      onChange={(e) => handleInputChange('iconName', e.target.value)}
                      placeholder="e.g., controller, music-note"
                      maxLength={50}
                    />
                  </div>
                  <div className="form-text">
                    Leave empty for default icon. See <a href="https://icons.getbootstrap.com/" target="_blank" rel="noopener noreferrer">Bootstrap Icons</a>.
                  </div>
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-outline-secondary" onClick={onClose}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    Creating...
                  </>
                ) : (
                  <>
                    <i className="bi bi-check-lg me-2"></i>
                    Create Category
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>,
    document.body
  );
};

export default Categories;
