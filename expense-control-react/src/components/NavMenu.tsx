import React, { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';

const NavMenu: React.FC = () => {
  const [collapsed, setCollapsed] = useState(true);

  const toggleNavMenu = () => {
    setCollapsed(!collapsed);
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

      <div className={`${collapsed ? 'collapse' : ''} nav-scrollable`}>
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
        </nav>
      </div>
    </>
  );
};

export default NavMenu;
