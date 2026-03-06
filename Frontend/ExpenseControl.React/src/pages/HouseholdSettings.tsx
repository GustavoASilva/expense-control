import React, { useState, useEffect } from 'react';
import { getUserHousehold } from '../services/api';
import { Household } from '../types/index';
import { useToast } from '../hooks/useToast';

const HouseholdSettings: React.FC = () => {
  const [household, setHousehold] = useState<Household | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [copied, setCopied] = useState(false);
  const { showToast } = useToast();

  useEffect(() => {
    const loadHousehold = async () => {
      try {
        const response = await getUserHousehold();
        setHousehold(response.household);
      } catch {
        showToast('Failed to load household information.', 'danger');
      } finally {
        setIsLoading(false);
      }
    };
    loadHousehold();
  }, [showToast]);

  const handleCopyInviteCode = async () => {
    if (!household) return;

    try {
      await navigator.clipboard.writeText(household.inviteId);
      setCopied(true);
      showToast('Invite code copied to clipboard!', 'success');
      setTimeout(() => setCopied(false), 2000);
    } catch {
      showToast('Failed to copy invite code.', 'danger');
    }
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (!household) {
    return (
      <div className="text-center py-5">
        <p className="text-muted">No household found.</p>
      </div>
    );
  }

  return (
    <div className="container-fluid py-4">
      <div className="d-flex align-items-center mb-4">
        <i className="bi bi-house-gear fs-4 me-3 text-primary"></i>
        <div>
          <h1 className="h3 mb-0">Household Settings</h1>
          <p className="text-muted small mb-0">Manage your household and invite members</p>
        </div>
      </div>

      <div className="row g-4">
        <div className="col-lg-6">
          <div className="card">
            <div className="card-body">
              <h5 className="card-title">
                <i className="bi bi-house-door me-2"></i>
                Household Info
              </h5>
              <div className="mt-3">
                <div className="mb-3">
                  <label className="form-label text-muted small mb-1">Name</label>
                  <div className="fw-medium">{household.name}</div>
                </div>
                <div>
                  <label className="form-label text-muted small mb-1">Created</label>
                  <div className="fw-medium">
                    {new Date(household.createdAt).toLocaleDateString()}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="card">
            <div className="card-body">
              <h5 className="card-title">
                <i className="bi bi-person-plus me-2"></i>
                Invite Members
              </h5>
              <p className="text-muted small">
                Share this invite code with others so they can join your household.
              </p>
              <div className="input-group mt-3">
                <input
                  type="text"
                  className="form-control font-monospace"
                  value={household.inviteId}
                  readOnly
                  style={{ fontSize: '0.85rem' }}
                />
                <button
                  className={`btn ${copied ? 'btn-success' : 'btn-outline-primary'}`}
                  onClick={handleCopyInviteCode}
                  type="button"
                >
                  <i className={`bi ${copied ? 'bi-check-lg' : 'bi-clipboard'} me-1`}></i>
                  {copied ? 'Copied!' : 'Copy'}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HouseholdSettings;
