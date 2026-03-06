import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { getCategories, createTransaction, updateTransaction } from '../services/api';
import { Category, TransactionForm as TxnPayload, TransactionType, Transaction } from '../types/index';
import { useToast } from '../hooks/useToast';

interface DialogConfiguration {
  isVisible: boolean;
  handleClose: () => void;
  refreshData: () => void;
  recordToUpdate?: Transaction | null;
}

interface FormDataContainer {
  dollarAmount: number;
  textualDescription: string;
  calendarValue: string;
  flowType: TransactionType;
  chosenCategoryId: string;
  additionalNotes: string;
}

interface ErrorContainer {
  dollarAmount: string;
  textualDescription: string;
  calendarValue: string;
  chosenCategoryId: string;
}

const createEmptyFormData = (): FormDataContainer => ({
  dollarAmount: 0,
  textualDescription: '',
  calendarValue: new Date().toISOString().split('T')[0],
  flowType: TransactionType.Expense,
  chosenCategoryId: '',
  additionalNotes: '',
});

const createEmptyErrors = (): ErrorContainer => ({
  dollarAmount: '',
  textualDescription: '',
  calendarValue: '',
  chosenCategoryId: '',
});

const TransactionForm: React.FC<DialogConfiguration> = ({ 
  isVisible, 
  handleClose, 
  refreshData, 
  recordToUpdate 
}) => {
  const { showToast } = useToast();
  const [availableCats, setAvailableCats] = useState<Category[]>([]);
  const [isFetchingCats, setIsFetchingCats] = useState(true);
  const [isSubmittingData, setIsSubmittingData] = useState(false);
  const [formContainer, setFormContainer] = useState<FormDataContainer>(createEmptyFormData());
  const [errorContainer, setErrorContainer] = useState<ErrorContainer>(createEmptyErrors());

  useEffect(() => {
    if (!isVisible) return;
    
    const bootstrap = async () => {
      setIsFetchingCats(true);
      try {
        const cats = await getCategories();
        setAvailableCats(cats);
      } catch (err) {
        console.error('Bootstrap error:', err);
      } finally {
        setIsFetchingCats(false);
      }
    };
    
    bootstrap();
    
    if (recordToUpdate) {
      setFormContainer({
        dollarAmount: recordToUpdate.amount,
        textualDescription: recordToUpdate.description,
        calendarValue: recordToUpdate.date,
        flowType: recordToUpdate.type,
        chosenCategoryId: recordToUpdate.categoryId,
        additionalNotes: recordToUpdate.notes || '',
      });
    } else {
      setFormContainer(createEmptyFormData());
    }
    
    setErrorContainer(createEmptyErrors());
  }, [isVisible, recordToUpdate]);

  const modifyFormField = (key: keyof FormDataContainer, value: string | number) => {
    setFormContainer(prev => ({ ...prev, [key]: value }));
    if (key in errorContainer) {
      setErrorContainer(prev => ({ ...prev, [key]: '' }));
    }
  };

  const handleTypeToggle = (newType: TransactionType) => {
    setFormContainer(prev => ({
      ...prev,
      flowType: newType,
      chosenCategoryId: '',
    }));
    setErrorContainer(prev => ({ ...prev, chosenCategoryId: '' }));
  };

  const validateAllFields = (): boolean => {
    const errors = createEmptyErrors();
    let isValid = true;

    if (isNaN(formContainer.dollarAmount) || formContainer.dollarAmount <= 0) {
      errors.dollarAmount = 'Amount must be a valid number greater than 0';
      isValid = false;
    }

    const cleaned = formContainer.textualDescription.trim();
    if (cleaned.length < 3) {
      errors.textualDescription = 'Description must be at least 3 characters';
      isValid = false;
    } else if (cleaned.length > 200) {
      errors.textualDescription = 'Description must be less than 200 characters';
      isValid = false;
    }

    if (!formContainer.calendarValue) {
      errors.calendarValue = 'Date is required';
      isValid = false;
    }

    if (!formContainer.chosenCategoryId) {
      errors.chosenCategoryId = 'Please select a category';
      isValid = false;
    }

    setErrorContainer(errors);
    return isValid;
  };

  const handleFormSubmission = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateAllFields() || isSubmittingData) return;

    setIsSubmittingData(true);

    const payload: TxnPayload = {
      amount: formContainer.dollarAmount,
      description: formContainer.textualDescription,
      date: formContainer.calendarValue,
      type: formContainer.flowType,
      categoryId: formContainer.chosenCategoryId,
      notes: formContainer.additionalNotes,
    };

    try {
      if (recordToUpdate) {
        await updateTransaction(recordToUpdate.id, payload);
      } else {
        await createTransaction(payload);
      }
      
      refreshData();
      handleClose();
    } catch (err) {
      console.error('Submission error:', err);
      showToast('Failed to save transaction. Please try again.', 'danger');
    } finally {
      setIsSubmittingData(false);
    }
  };

  const filteredCatList = availableCats.filter(category => category.type === formContainer.flowType);

  if (!isVisible) return null;

  return createPortal(
    <div 
      className="modal show d-block" 
      tabIndex={-1} 
      style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
      onClick={handleClose}
    >
      <div 
        className="modal-dialog modal-dialog-centered modal-lg" 
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal-content">
          <form onSubmit={handleFormSubmission}>
            <div className="modal-header">
              <h5 className="modal-title">
                <i className="bi bi-receipt me-2"></i>
                {recordToUpdate ? 'Edit Transaction' : 'New Transaction'}
              </h5>
              <button type="button" className="btn-close" onClick={handleClose} aria-label="Close"></button>
            </div>
            <div className="modal-body">
              {isFetchingCats ? (
                <div className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              ) : (
                <>
                  <div className="mb-4">
                    <label className="form-label">Transaction Type</label>
                    <div className="type-toggle">
                      <input
                        type="radio"
                        className="btn-check"
                        name="txnTypeRadio"
                        id="expenseTypeBtn"
                        checked={formContainer.flowType === TransactionType.Expense}
                        onChange={() => handleTypeToggle(TransactionType.Expense)}
                      />
                      <label className="btn btn-outline-danger" htmlFor="expenseTypeBtn">
                        <i className="bi bi-arrow-up-right me-2"></i>Expense
                      </label>

                      <input
                        type="radio"
                        className="btn-check"
                        name="txnTypeRadio"
                        id="incomeTypeBtn"
                        checked={formContainer.flowType === TransactionType.Income}
                        onChange={() => handleTypeToggle(TransactionType.Income)}
                      />
                      <label className="btn btn-outline-success" htmlFor="incomeTypeBtn">
                        <i className="bi bi-arrow-down-left me-2"></i>Income
                      </label>
                    </div>
                  </div>
                  <div className="row g-4">
                    <div className="col-md-6">
                      <label htmlFor="dollarInput" className="form-label">Amount</label>
                      <div className="input-group">
                        <span className="input-group-text">$</span>
                        <input
                          type="number"
                          className={`form-control ${errorContainer.dollarAmount ? 'is-invalid' : ''}`}
                          id="dollarInput"
                          value={formContainer.dollarAmount || ''}
                          onChange={(e) => {
                            const val = e.target.value;
                            if (val === '') {
                              modifyFormField('dollarAmount', 0);
                            } else {
                              const num = parseFloat(val);
                              if (!isNaN(num) && num >= 0) {
                                modifyFormField('dollarAmount', num);
                              }
                            }
                          }}
                          onKeyDown={(e) => {
                            if (['e', 'E', '+', '-'].includes(e.key)) {
                              e.preventDefault();
                            }
                          }}
                          step="0.01"
                          min="0.01"
                          placeholder="0.00"
                        />
                        {errorContainer.dollarAmount && (
                          <div className="invalid-feedback">{errorContainer.dollarAmount}</div>
                        )}
                      </div>
                    </div>
                    <div className="col-md-6">
                      <label htmlFor="calendarInput" className="form-label">Date</label>
                      <input
                        type="date"
                        className={`form-control ${errorContainer.calendarValue ? 'is-invalid' : ''}`}
                        id="calendarInput"
                        value={formContainer.calendarValue}
                        onChange={(e) => modifyFormField('calendarValue', e.target.value)}
                      />
                      {errorContainer.calendarValue && (
                        <div className="invalid-feedback">{errorContainer.calendarValue}</div>
                      )}
                    </div>
                    <div className="col-12">
                      <label htmlFor="descInput" className="form-label">Description</label>
                      <input
                        type="text"
                        className={`form-control ${errorContainer.textualDescription ? 'is-invalid' : ''}`}
                        id="descInput"
                        value={formContainer.textualDescription}
                        onChange={(e) => modifyFormField('textualDescription', e.target.value)}
                        placeholder="What was this transaction for?"
                        maxLength={200}
                      />
                      {errorContainer.textualDescription && (
                        <div className="invalid-feedback">{errorContainer.textualDescription}</div>
                      )}
                    </div>
                    <div className="col-12">
                      <label htmlFor="catSelect" className="form-label">Category</label>
                      <select
                        className={`form-select ${errorContainer.chosenCategoryId ? 'is-invalid' : ''}`}
                        id="catSelect"
                        value={formContainer.chosenCategoryId}
                        onChange={(e) => modifyFormField('chosenCategoryId', e.target.value)}
                      >
                        <option value="">Select a category</option>
                        {filteredCatList.map((cat) => (
                          <option key={cat.id} value={cat.id}>{cat.name}</option>
                        ))}
                      </select>
                      {errorContainer.chosenCategoryId && (
                        <div className="invalid-feedback">{errorContainer.chosenCategoryId}</div>
                      )}
                    </div>
                    <div className="col-12">
                      <label htmlFor="notesArea" className="form-label">
                        Notes <span className="text-muted">(optional)</span>
                      </label>
                      <textarea
                        className="form-control"
                        id="notesArea"
                        value={formContainer.additionalNotes}
                        onChange={(e) => modifyFormField('additionalNotes', e.target.value)}
                        rows={3}
                        placeholder="Add any additional notes..."
                        maxLength={1000}
                      />
                    </div>
                  </div>
                </>
              )}
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-outline-secondary" onClick={handleClose}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={isSubmittingData || isFetchingCats}>
                {isSubmittingData ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    Saving...
                  </>
                ) : (
                  <>
                    <i className="bi bi-check-lg me-2"></i>
                    {recordToUpdate ? 'Save Changes' : 'Create Transaction'}
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

export default TransactionForm;
