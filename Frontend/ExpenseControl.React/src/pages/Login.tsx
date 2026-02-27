import React, { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { useNavigate } from 'react-router-dom';

const Login: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [mfaCode, setMfaCode] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { login, completeNewPassword, verifyMfaCode, requiresPasswordReset, requiresMfa, mfaMethod } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      if (requiresMfa) {
        await verifyMfaCode(mfaCode.trim());
        navigate('/');
        return;
      }

      if (requiresPasswordReset) {
        if (newPassword !== confirmNewPassword) {
          setError('New password and confirmation do not match.');
          return;
        }

        const result = await completeNewPassword(newPassword);
        if (!result.requiresMfa) {
          navigate('/');
        }
        return;
      }

      const result = await login(username, password);
      if (result.requiresPasswordReset || result.requiresMfa) {
        setError('');
        return;
      }

      navigate('/');
    } catch {
      if (requiresMfa) {
        setError('Invalid verification code. Please try again.');
      } else if (requiresPasswordReset) {
        setError('Unable to set a new password. Please ensure it meets the password policy.');
      } else {
        setError('Invalid username or password. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const submitButtonLabel = () => {
    if (requiresMfa) {
      return isSubmitting ? 'Verifying code...' : 'Verify Code';
    }

    if (requiresPasswordReset) {
      return isSubmitting ? 'Updating password...' : 'Set New Password';
    }

    return isSubmitting ? 'Signing in...' : 'Sign In';
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
              <i className="bi bi-wallet2 text-white fs-3"></i>
            </div>
            <h1 className="h4 mb-1">ExpenseCtrl</h1>
            <p className="text-muted small">Sign in to manage your expenses</p>
          </div>

          {error && (
            <div className="alert alert-danger py-2 small" role="alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            {!requiresPasswordReset && !requiresMfa ? (
              <>
                <div className="mb-3">
                  <label htmlFor="username" className="form-label">Username</label>
                  <input
                    type="text"
                    id="username"
                    className="form-control"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                    autoFocus
                    disabled={isSubmitting}
                  />
                </div>

                <div className="mb-4">
                  <label htmlFor="password" className="form-label">Password</label>
                  <input
                    type="password"
                    id="password"
                    className="form-control"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    disabled={isSubmitting}
                  />
                </div>
              </>
            ) : requiresPasswordReset ? (
              <>
                <div className="alert alert-info py-2 small" role="status">
                  This account requires a new password before you can continue.
                </div>

                <div className="mb-3">
                  <label htmlFor="newPassword" className="form-label">New Password</label>
                  <input
                    type="password"
                    id="newPassword"
                    className="form-control"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    required
                    autoFocus
                    disabled={isSubmitting}
                  />
                </div>

                <div className="mb-4">
                  <label htmlFor="confirmNewPassword" className="form-label">Confirm New Password</label>
                  <input
                    type="password"
                    id="confirmNewPassword"
                    className="form-control"
                    value={confirmNewPassword}
                    onChange={(e) => setConfirmNewPassword(e.target.value)}
                    required
                    disabled={isSubmitting}
                  />
                </div>
              </>
            ) : (
              <>
                <div className="alert alert-info py-2 small" role="status">
                  Enter the {mfaMethod === 'SMS' ? 'SMS' : 'authenticator app'} verification code.
                </div>

                <div className="mb-4">
                  <label htmlFor="mfaCode" className="form-label">Verification Code</label>
                  <input
                    type="text"
                    id="mfaCode"
                    className="form-control"
                    value={mfaCode}
                    onChange={(e) => setMfaCode(e.target.value)}
                    required
                    autoFocus
                    inputMode="numeric"
                    autoComplete="one-time-code"
                    disabled={isSubmitting}
                  />
                </div>
              </>
            )}

            <button
              type="submit"
              className="btn btn-primary w-100"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  {submitButtonLabel()}
                </>
              ) : (
                submitButtonLabel()
              )}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Login;
