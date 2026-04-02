import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { Saving, SavingForm as SavingFormType } from '../types/index';
import { createSaving, updateSaving } from '../services/api';
import { useToast } from '../hooks/useToast';

interface SavingFormProps {
  show: boolean;
  onClose: () => void;
  onSave: () => void;
  editSaving?: Saving | null;
}

const SavingForm: React.FC<SavingFormProps> = ({ show, onClose, onSave, editSaving }) => {
  const { showToast } = useToast();
  const [isSaving, setIsSaving] = useState(false);
  const [form, setForm] = useState<SavingFormType>({
    name: '',
    description: '',
    currentAmount: 0,
    targetAmount: undefined,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (show) {
      if (editSaving) {
        setForm({
          name: editSaving.name,
          description: editSaving.description ?? '',
          currentAmount: editSaving.currentAmount,
          targetAmount: editSaving.targetAmount,
        });
      } else {
        setForm({
          name: '',
          description: '',
          currentAmount: 0,
          targetAmount: undefined,
        });
      }
      setErrors({});
    }
  }, [show, editSaving]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    let parsedValue: string | number | undefined = value;

    if (name === 'currentAmount') {
      parsedValue = parseFloat(value) || 0;
    } else if (name === 'targetAmount') {
      parsedValue = value === '' ? undefined : parseFloat(value) || 0;
    }

    setForm((prev) => ({
      ...prev,
      [name]: parsedValue,
    }));

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

    if (!form.name.trim()) {
      newErrors.name = 'Name is required';
    }

    if (isNaN(form.currentAmount) || form.currentAmount < 0) {
      newErrors.currentAmount = 'Current amount must be a valid number of 0 or more';
    }

    if (form.targetAmount !== undefined && (isNaN(form.targetAmount) || form.targetAmount <= 0)) {
      newErrors.targetAmount = 'Target amount must be a valid number greater than 0';
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
      const payload: SavingFormType = {
        name: form.name.trim(),
        description: form.description?.trim() || undefined,
        currentAmount: form.currentAmount,
        targetAmount: form.targetAmount,
      };

      if (editSaving) {
        await updateSaving(editSaving.id, payload);
      } else {
        await createSaving(payload);
      }
      onSave();
      onClose();
    } catch (error) {
      console.error('Failed to save saving fund:', error);
      showToast('Failed to save. Please try again.', 'danger');
    } finally {
      setIsSaving(false);
    }
  };

  if (!show) return null;

  return createPortal(
    <div
      className="modal show d-block"
      tabIndex={-1}
      style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
      onClick={onClose}
    >
      <div className="modal-dialog modal-dialog-centered" onClick={(e) => e.stopPropagation()}>
        <div className="modal-content">
          <form onSubmit={handleSubmit}>
            <div className="modal-header">
              <h5 className="modal-title">
                <i className="bi bi-piggy-bank me-2"></i>
                {editSaving ? 'Edit Savings Fund' : 'New Savings Fund'}
              </h5>
              <button type="button" className="btn-close" onClick={onClose} aria-label="Close"></button>
            </div>
            <div className="modal-body">
              <div className="row g-4">
                <div className="col-12">
                  <label htmlFor="name" className="form-label">
                    Name
                  </label>
                  <input
                    type="text"
                    className={`form-control ${errors.name ? 'is-invalid' : ''}`}
                    id="name"
                    name="name"
                    value={form.name}
                    onChange={handleInputChange}
                    placeholder="e.g. Emergency Fund"
                    maxLength={200}
                  />
                  {errors.name && (
                    <div className="invalid-feedback">{errors.name}</div>
                  )}
                </div>

                <div className="col-12">
                  <label htmlFor="description" className="form-label">
                    Description <span className="text-muted">(optional)</span>
                  </label>
                  <input
                    type="text"
                    className="form-control"
                    id="description"
                    name="description"
                    value={form.description ?? ''}
                    onChange={handleInputChange}
                    placeholder="e.g. 3 months of expenses"
                    maxLength={500}
                  />
                </div>

                <div className="col-md-6">
                  <label htmlFor="currentAmount" className="form-label">
                    Current Amount
                  </label>
                  <div className="input-group">
                    <span className="input-group-text">$</span>
                    <input
                      type="number"
                      className={`form-control ${errors.currentAmount ? 'is-invalid' : ''}`}
                      id="currentAmount"
                      name="currentAmount"
                      value={form.currentAmount || ''}
                      onChange={handleInputChange}
                      onKeyDown={(e) => {
                        if (['e', 'E', '+', '-'].includes(e.key)) {
                          e.preventDefault();
                        }
                      }}
                      step="0.01"
                      min="0"
                      placeholder="0.00"
                    />
                  </div>
                  {errors.currentAmount && (
                    <div className="invalid-feedback d-block">{errors.currentAmount}</div>
                  )}
                </div>

                <div className="col-md-6">
                  <label htmlFor="targetAmount" className="form-label">
                    Target Amount <span className="text-muted">(optional)</span>
                  </label>
                  <div className="input-group">
                    <span className="input-group-text">$</span>
                    <input
                      type="number"
                      className={`form-control ${errors.targetAmount ? 'is-invalid' : ''}`}
                      id="targetAmount"
                      name="targetAmount"
                      value={form.targetAmount ?? ''}
                      onChange={handleInputChange}
                      onKeyDown={(e) => {
                        if (['e', 'E', '+', '-'].includes(e.key)) {
                          e.preventDefault();
                        }
                      }}
                      step="0.01"
                      min="0.01"
                      placeholder="0.00"
                    />
                  </div>
                  {errors.targetAmount && (
                    <div className="invalid-feedback d-block">{errors.targetAmount}</div>
                  )}
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-outline-secondary" onClick={onClose}>
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
                    {editSaving ? 'Save Changes' : 'Create Savings Fund'}
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

export default SavingForm;
