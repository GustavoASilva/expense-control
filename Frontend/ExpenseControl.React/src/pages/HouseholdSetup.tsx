import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { createHousehold, joinHousehold } from '../services/api';

type SetupMode = 'choose' | 'create' | 'join';

const HouseholdSetup: React.FC = () => {
  const [mode, setMode] = useState<SetupMode>('choose');
  const [name, setName] = useState('');
  const [inviteCode, setInviteCode] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { checkHousehold } = useAuth();
  const navigate = useNavigate();

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const trimmed = name.trim();
    if (!trimmed) {
      setError('Household name is required.');
      return;
    }

    if (trimmed.length < 2) {
      setError('Household name must be at least 2 characters.');
      return;
    }

    if (trimmed.length > 200) {
      setError('Household name must be at most 200 characters.');
      return;
    }

    setIsSubmitting(true);

    try {
      await createHousehold(trimmed);
      await checkHousehold();
      navigate('/');
    } catch {
      setError('Failed to create household. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleJoin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const trimmed = inviteCode.trim();
    if (!trimmed) {
      setError('Invite code is required.');
      return;
    }

    // Validate UUID format
    const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    if (!uuidRegex.test(trimmed)) {
      setError('Please enter a valid invite code.');
      return;
    }

    setIsSubmitting(true);

    try {
      await joinHousehold(trimmed);
      await checkHousehold();
      navigate('/');
    } catch {
      setError('Failed to join household. Please check the invite code and try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleBack = () => {
    setMode('choose');
    setError('');
    setName('');
    setInviteCode('');
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
            <p className="text-muted small">
              {mode === 'choose' && 'Create a new household or join an existing one'}
              {mode === 'create' && 'Create a new household to get started'}
              {mode === 'join' && 'Enter the invite code to join a household'}
            </p>
          </div>

          {error && (
            <div className="alert alert-danger py-2 small" role="alert">
              {error}
            </div>
          )}

          {mode === 'choose' && (
            <div className="d-grid gap-3">
              <button
                className="btn btn-primary py-3"
                onClick={() => setMode('create')}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Create New Household
              </button>
              <button
                className="btn btn-outline-primary py-3"
                onClick={() => setMode('join')}
              >
                <i className="bi bi-people me-2"></i>
                Join Existing Household
              </button>
            </div>
          )}

          {mode === 'create' && (
            <form onSubmit={handleCreate}>
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
                  maxLength={200}
                  autoFocus
                  disabled={isSubmitting}
                />
              </div>

              <div className="d-grid gap-2">
                <button
                  type="submit"
                  className="btn btn-primary"
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
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={handleBack}
                  disabled={isSubmitting}
                >
                  Back
                </button>
              </div>
            </form>
          )}

          {mode === 'join' && (
            <form onSubmit={handleJoin}>
              <div className="mb-4">
                <label htmlFor="inviteCode" className="form-label">Invite Code</label>
                <input
                  type="text"
                  id="inviteCode"
                  className="form-control"
                  value={inviteCode}
                  onChange={(e) => setInviteCode(e.target.value)}
                  placeholder="Enter invite code"
                  required
                  autoFocus
                  disabled={isSubmitting}
                />
                <div className="form-text">
                  Ask a household member for the invite code.
                </div>
              </div>

              <div className="d-grid gap-2">
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                      Joining...
                    </>
                  ) : (
                    'Join Household'
                  )}
                </button>
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={handleBack}
                  disabled={isSubmitting}
                >
                  Back
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};

export default HouseholdSetup;
