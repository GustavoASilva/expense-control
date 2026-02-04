import React from 'react';

interface StatCardProps {
  title: string;
  value: number;
  icon?: string;
  inverted?: boolean;
  previousValue?: number;
  showChange?: boolean;
  type?: 'income' | 'expense' | 'balance';
}

const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  icon = 'bi-cash',
  previousValue,
  showChange = true,
  type = 'balance',
}) => {
  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const calculateChange = (): { change: number; isPositive: boolean } | null => {
    if (!previousValue || !showChange) return null;

    const previousAbs = Math.abs(previousValue);
    const change = previousAbs > 0 ? ((value - previousValue) / previousAbs) * 100 : 0;
    const isPositive = change > 0;

    return { change, isPositive };
  };

  const changeData = calculateChange();

  return (
    <div className={`stat-card ${type}`}>
      <div className="stat-card-icon">
        <i className={`bi ${icon}`}></i>
      </div>
      <div className="stat-card-label">{title}</div>
      <div className="stat-card-value">{formatCurrency(value)}</div>
      {changeData && changeData.change !== 0 && (
        <div className={`stat-card-change ${changeData.isPositive ? 'positive' : 'negative'}`}>
          <i className={`bi ${changeData.isPositive ? 'bi-arrow-up' : 'bi-arrow-down'}`}></i>
          <span>{Math.abs(changeData.change).toFixed(1)}%</span>
          <span style={{ opacity: 0.7, marginLeft: '0.25rem' }}>vs prev</span>
        </div>
      )}
    </div>
  );
};

export default StatCard;
