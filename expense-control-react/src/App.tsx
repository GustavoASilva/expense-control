import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Transactions from './pages/Transactions';
import TransactionForm from './pages/TransactionForm';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="transactions" element={<Transactions />} />
          <Route path="transactions/new" element={<TransactionForm />} />
          <Route path="transactions/:id" element={<TransactionForm />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
