import React from 'react';

interface StatCardProps {
  title: string;
  value: number;
  icon?: string;
  inverted?: boolean;
  previousValue?: number;
  showChange?: boolean;
}

const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  icon = 'bi-cash',
  inverted = false,
  previousValue,
  showChange = true,
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
    <div className={`card shadow-sm mb-4 ${inverted ? 'bg-primary text-white' : ''}`}>
      <div className="card-body">
        <div className="d-flex justify-content-between align-items-center">
          <div>
            <h6 className={`card-subtitle mb-2 ${inverted ? 'text-white-50' : 'text-muted'}`}>
              {title}
            </h6>
            <h2 className="card-title mb-0">{formatCurrency(value)}</h2>
          </div>
          <div className={inverted ? 'text-white-50' : 'text-muted'}>
            <i className={`bi ${icon} fs-1`}></i>
          </div>
        </div>
        {changeData && (
          <div className="mt-3">
            <small className={inverted ? 'text-white-50' : 'text-muted'}>
              vs previous period{' '}
              <span className={changeData.isPositive ? 'text-success' : 'text-danger'}>
                {changeData.isPositive ? '+' : ''}
                {changeData.change.toFixed(1)}%
              </span>
            </small>
          </div>
        )}
      </div>
    </div>
  );
};

export default StatCard;
