import React, { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';

const NavMenu: React.FC = () => {
  const [collapsed, setCollapsed] = useState(true);

  const toggleNavMenu = () => {
    setCollapsed(!collapsed);
  };

  return (
    <>
      <div className="top-row ps-3 navbar navbar-dark">
        <div className="container-fluid">
          <Link className="navbar-brand" to="/">
            Expense Control
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

      <div className={`${collapsed ? 'collapse' : ''} nav-scrollable`}>
        <nav className="flex-column">
          <div className="nav-item px-3">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/"
              end
            >
              <i className="bi bi-house-door me-2"></i> Home
            </NavLink>
          </div>
          <div className="nav-item px-3">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/transactions"
            >
              <i className="bi bi-credit-card me-2"></i> Transactions
            </NavLink>
          </div>
          <div className="nav-item px-3">
            <NavLink
              className={({ isActive }) =>
                `nav-link ${isActive ? 'active' : ''}`
              }
              to="/budgets"
            >
              <i className="bi bi-wallet2 me-2"></i> Budgets
            </NavLink>
          </div>
        </nav>
      </div>
    </>
  );
};

export default NavMenu;
