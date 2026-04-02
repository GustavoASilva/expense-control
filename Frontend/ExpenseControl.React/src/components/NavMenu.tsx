import React, { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const NavMenu: React.FC = () => {
  const [collapsed, setCollapsed] = useState(false);
  const { user, householdName, logout } = useAuth();

  const toggleNavMenu = () => {
    setCollapsed(!collapsed);
  };

  const handleLogout = async () => {
    await logout();
  };

  return (
    <>
      <div className="top-row navbar navbar-dark">
        <div className="container-fluid">
          <Link className="navbar-brand" to="/">
            <span className="brand-icon">
              <i className="bi bi-wallet2"></i>
            </span>
            ExpenseCtrl
          </Link>
          <button
            title="Navigation menu"
            className="navbar-toggler"
            onClick={toggleNavMenu}
          >
            <span className="navbar-toggler-icon"></span>
          </button>
        </div>
      </div>

      <div className={`${collapsed ? 'collapse' : ''} nav-scrollable show`}>
        {householdName && (
          <div style={{
            padding: '0.75rem 1rem',
            borderBottom: '1px solid rgba(255, 255, 255, 0.08)',
          }}>
            <div style={{
              color: 'rgba(255, 255, 255, 0.5)',
              fontSize: '0.6875rem',
              textTransform: 'uppercase',
              letterSpacing: '0.05em',
              marginBottom: '0.25rem',
            }}>
              Household
            </div>
            <div style={{
              color: 'rgba(255, 255, 255, 0.85)',
              fontSize: '0.875rem',
              fontWeight: 500,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}>
              <i className="bi bi-house-door me-2"></i>
              {householdName}
            </div>
          </div>
        )}

        <nav className="flex-column">
          <div className="nav-item">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/"
              end
            >
              <i className="bi bi-grid-1x2-fill"></i>
              <span>Dashboard</span>
            </NavLink>
          </div>
          <div className="nav-item">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/transactions"
            >
              <i className="bi bi-arrow-left-right"></i>
              <span>Transactions</span>
            </NavLink>
          </div>
          <div className="nav-item">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/budgets"
            >
              <i className="bi bi-pie-chart-fill"></i>
              <span>Budgets</span>
            </NavLink>
          </div>
          <div className="nav-item">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/savings"
            >
              <i className="bi bi-piggy-bank-fill"></i>
              <span>Savings</span>
            </NavLink>
          </div>
          <div className="nav-item">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/categories"
            >
              <i className="bi bi-tags"></i>
              <span>Categories</span>
            </NavLink>
          </div>
          <div className="nav-item">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/household"
            >
              <i className="bi bi-house-gear"></i>
              <span>Household</span>
            </NavLink>
          </div>
        </nav>

        {user && (
          <div className="nav-user-section" style={{
            borderTop: '1px solid rgba(255, 255, 255, 0.08)',
            padding: '1rem 0.75rem',
            marginTop: 'auto',
          }}>
            <div style={{
              color: 'rgba(255, 255, 255, 0.6)',
              fontSize: '0.8125rem',
              padding: '0 1rem',
              marginBottom: '0.5rem',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}>
              <i className="bi bi-person-circle me-2"></i>
              {user.username}
            </div>
            <button
              className="nav-link w-100 text-start border-0 bg-transparent"
              onClick={handleLogout}
              style={{
                color: 'rgba(255, 255, 255, 0.7)',
                borderRadius: 'var(--radius-md)',
                height: '2.75rem',
                display: 'flex',
                alignItems: 'center',
                padding: '0 1rem',
                cursor: 'pointer',
                fontWeight: 500,
              }}
            >
              <i className="bi bi-box-arrow-left" style={{
                fontSize: '1.125rem',
                width: '1.5rem',
                marginRight: '0.75rem',
                opacity: 0.8,
              }}></i>
              <span>Sign Out</span>
            </button>
          </div>
        )}
      </div>
    </>
  );
};

export default NavMenu;
