import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { createHousehold } from '../services/api';

const HouseholdSetup: React.FC = () => {
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { checkHousehold } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name.trim()) {
      setError('Household name is required.');
      return;
    }

    setIsSubmitting(true);

    try {
      await createHousehold(name.trim());
      await checkHousehold();
      navigate('/');
    } catch {
      setError('Failed to create household. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="login-container d-flex align-items-center justify-content-center min-vh-100">
      <div className="login-card card" style={{ maxWidth: '420px', width: '100%' }}>
        <div className="card-body p-4">
          <div className="text-center mb-4">
            <div className="brand-icon mx-auto mb-3" style={{
              width: '56px',
              height: '56px',
              background: 'var(--primary-gradient)',
              borderRadius: 'var(--radius-lg)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}>
              <i className="bi bi-house-door text-white fs-3"></i>
            </div>
            <h1 className="h4 mb-1">Set Up Your Household</h1>
            <p className="text-muted small">Create a household to get started</p>
          </div>

          {error && (
            <div className="alert alert-danger py-2 small" role="alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label htmlFor="householdName" className="form-label">Household Name</label>
              <input
                type="text"
                id="householdName"
                className="form-control"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g., Smith Family"
                required
                autoFocus
                disabled={isSubmitting}
              />
            </div>

            <button
              type="submit"
              className="btn btn-primary w-100"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  Creating...
                </>
              ) : (
                'Create Household'
              )}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default HouseholdSetup;
