import React, { useState, useEffect } from 'react';
import { getSavings, deleteSaving } from '../services/api';
import { Saving } from '../types/index';
import SavingForm from '../components/SavingForm';
import { useToast } from '../hooks/useToast';
import { useConfirm } from '../hooks/useConfirm';

const Savings: React.FC = () => {
  const [isLoading, setIsLoading] = useState(true);
  const [savings, setSavings] = useState<Saving[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editSaving, setEditSaving] = useState<Saving | null>(null);
  const { showToast } = useToast();
  const { confirm } = useConfirm();

  useEffect(() => {
    loadSavings();
  }, []);

  const loadSavings = async () => {
    setIsLoading(true);
    try {
      const data = await getSavings();
      setSavings(data);
    } catch (error) {
      console.error('Failed to load savings:', error);
      setSavings([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateSaving = () => {
    setEditSaving(null);
    setShowForm(true);
  };

  const handleEditSaving = (saving: Saving) => {
    setEditSaving(saving);
    setShowForm(true);
  };

  const handleDeleteSaving = async (saving: Saving, e: React.MouseEvent) => {
    e.stopPropagation();
    const confirmed = await confirm({
      title: 'Delete Savings Fund',
      message: `Are you sure you want to delete "${saving.name}"? This action cannot be undone.`,
      confirmText: 'Delete',
      variant: 'danger',
    });

    if (!confirmed) return;

    try {
      await deleteSaving(saving.id);
      showToast(`"${saving.name}" deleted successfully.`, 'success');
      loadSavings();
    } catch (error) {
      console.error('Failed to delete saving:', error);
      showToast('Failed to delete savings fund. Please try again.', 'danger');
    }
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditSaving(null);
  };

  const handleFormSave = () => {
    loadSavings();
  };

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const getProgressPercent = (saving: Saving): number => {
    if (!saving.targetAmount || saving.targetAmount <= 0) return 0;
    return Math.min((saving.currentAmount / saving.targetAmount) * 100, 100);
  };

  const getProgressColor = (percent: number): string => {
    if (percent >= 100) return 'success';
    if (percent >= 75) return 'info';
    if (percent >= 50) return 'primary';
    return 'warning';
  };

  const totalSaved = savings.reduce((sum, s) => sum + s.currentAmount, 0);
  const totalTarget = savings
    .filter((s) => s.targetAmount != null)
    .reduce((sum, s) => sum + (s.targetAmount ?? 0), 0);

  return (
    <div className="fade-in">
      <div className="page-header">
        <h1 className="page-title">Savings</h1>
        <button className="btn btn-primary" onClick={handleCreateSaving}>
          <i className="bi bi-plus-lg me-2"></i>New Savings Fund
        </button>
      </div>

      {!isLoading && savings.length > 0 && (
        <div className="row g-3 mb-4">
          <div className="col-sm-6 col-lg-3">
            <div className="card">
              <div className="card-body">
                <div className="text-muted mb-1" style={{ fontSize: '0.8125rem' }}>Total Saved</div>
                <div className="fw-semibold fs-5 text-success">{formatCurrency(totalSaved)}</div>
              </div>
            </div>
          </div>
          {totalTarget > 0 && (
            <div className="col-sm-6 col-lg-3">
              <div className="card">
                <div className="card-body">
                  <div className="text-muted mb-1" style={{ fontSize: '0.8125rem' }}>Total Target</div>
                  <div className="fw-semibold fs-5">{formatCurrency(totalTarget)}</div>
                </div>
              </div>
            </div>
          )}
          <div className="col-sm-6 col-lg-3">
            <div className="card">
              <div className="card-body">
                <div className="text-muted mb-1" style={{ fontSize: '0.8125rem' }}>Funds</div>
                <div className="fw-semibold fs-5">{savings.length}</div>
              </div>
            </div>
          </div>
        </div>
      )}

      {isLoading ? (
        <div className="d-flex align-items-center justify-content-center" style={{ minHeight: '300px' }}>
          <div className="text-center">
            <div className="spinner-border text-primary mb-3" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
            <p className="text-muted mb-0">Loading savings...</p>
          </div>
        </div>
      ) : savings.length === 0 ? (
        <div className="card">
          <div className="card-body">
            <div className="empty-state">
              <div className="empty-state-icon">
                <i className="bi bi-piggy-bank"></i>
              </div>
              <div className="empty-state-title">No savings funds yet</div>
              <div className="empty-state-description">
                Create a savings fund to track money stored separately from your daily balance
              </div>
              <button className="btn btn-primary" onClick={handleCreateSaving}>
                <i className="bi bi-plus-lg me-2"></i>Create Savings Fund
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="row g-4">
          {savings.map((saving) => {
            const percent = getProgressPercent(saving);
            const progressColor = getProgressColor(percent);
            const hasTarget = saving.targetAmount != null && saving.targetAmount > 0;
            const remaining = hasTarget ? (saving.targetAmount ?? 0) - saving.currentAmount : null;

            return (
              <div key={saving.id} className="col-md-6 col-lg-4">
                <div
                  className="card h-100"
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleEditSaving(saving)}
                >
                  <div className="card-body">
                    <div className="d-flex justify-content-between align-items-start mb-3">
                      <div className="flex-grow-1 me-2" style={{ minWidth: 0 }}>
                        <h5 className="mb-1 text-truncate">{saving.name}</h5>
                        {saving.description && (
                          <div className="text-muted text-truncate" style={{ fontSize: '0.8125rem' }}>
                            {saving.description}
                          </div>
                        )}
                      </div>
                      <div className="d-flex gap-1 flex-shrink-0">
                        <button
                          className="btn btn-sm btn-outline-primary"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleEditSaving(saving);
                          }}
                          title="Edit"
                        >
                          <i className="bi bi-pencil"></i>
                        </button>
                        <button
                          className="btn btn-sm btn-outline-danger"
                          onClick={(e) => handleDeleteSaving(saving, e)}
                          title="Delete"
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      </div>
                    </div>

                    <div className="mb-3">
                      <div className="fw-semibold fs-5 text-success mb-1">
                        {formatCurrency(saving.currentAmount)}
                      </div>
                      {hasTarget && (
                        <>
                          <div className="d-flex justify-content-between mb-2">
                            <span className="text-muted" style={{ fontSize: '0.8125rem' }}>
                              Saved
                            </span>
                            <span className="text-muted" style={{ fontSize: '0.8125rem' }}>
                              {formatCurrency(saving.targetAmount ?? 0)} goal
                            </span>
                          </div>
                          <div className="progress" style={{ height: '8px' }}>
                            <div
                              className={`progress-bar bg-${progressColor}`}
                              role="progressbar"
                              style={{ width: `${percent}%` }}
                              aria-valuenow={percent}
                              aria-valuemin={0}
                              aria-valuemax={100}
                            />
                          </div>
                        </>
                      )}
                    </div>

                    {hasTarget && (
                      <div className="d-flex justify-content-between align-items-center">
                        <div>
                          <span className="text-muted" style={{ fontSize: '0.75rem' }}>
                            {remaining !== null && remaining >= 0 ? 'Still needed' : 'Exceeded goal by'}
                          </span>
                          <div
                            className={`fw-semibold ${remaining !== null && remaining < 0 ? 'text-success' : ''}`}
                          >
                            {remaining !== null ? formatCurrency(Math.abs(remaining)) : '—'}
                          </div>
                        </div>
                        <div className="text-end">
                          <span className="text-muted" style={{ fontSize: '0.75rem' }}>Progress</span>
                          <div className={`fw-semibold text-${progressColor}`}>
                            {percent.toFixed(0)}%
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <SavingForm
        show={showForm}
        onClose={handleFormClose}
        onSave={handleFormSave}
        editSaving={editSaving}
      />
    </div>
  );
};

export default Savings;
