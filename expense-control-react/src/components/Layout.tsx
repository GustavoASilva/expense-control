import React from 'react';
import { Outlet } from 'react-router-dom';
import NavMenu from './NavMenu';

const Layout: React.FC = () => {
  return (
    <div className="page">
      <div className="sidebar">
        <NavMenu />
      </div>

      <main>
        <article className="content px-4">
          <Outlet />
        </article>
      </main>
    </div>
  );
};

export default Layout;
