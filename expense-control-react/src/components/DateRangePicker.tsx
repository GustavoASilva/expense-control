import React from 'react';

interface DatePeriod {
  label: string;
  start: string;
  end: string;
}

interface DateRangePickerProps {
  startDate?: string;
  endDate?: string;
  small?: boolean;
  onChanged: (range: { start: string; end: string }) => void;
}

const DateRangePicker: React.FC<DateRangePickerProps> = ({
  startDate,
  endDate,
  onChanged,
}) => {
  const today = new Date();
  const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

  const formatDate = (date: Date): string => {
    return date.toISOString().split('T')[0];
  };

  const addDays = (date: Date, days: number): Date => {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
  };

  const addMonths = (date: Date, months: number): Date => {
    const result = new Date(date);
    result.setMonth(result.getMonth() + months);
    return result;
  };

  const periods: DatePeriod[] = [
    {
      label: '7 days',
      start: formatDate(addDays(today, -6)),
      end: formatDate(today),
    },
    {
      label: '30 days',
      start: formatDate(addDays(today, -29)),
      end: formatDate(today),
    },
    {
      label: 'This month',
      start: formatDate(firstDayOfMonth),
      end: formatDate(today),
    },
    {
      label: 'Last month',
      start: formatDate(addMonths(firstDayOfMonth, -1)),
      end: formatDate(addDays(firstDayOfMonth, -1)),
    },
    {
      label: '3 months',
      start: formatDate(addMonths(firstDayOfMonth, -2)),
      end: formatDate(today),
    },
    {
      label: 'This year',
      start: formatDate(new Date(today.getFullYear(), 0, 1)),
      end: formatDate(today),
    },
  ];

  const isSelected = (period: DatePeriod): boolean => {
    return startDate === period.start && endDate === period.end;
  };

  const handlePeriodClick = (period: DatePeriod) => {
    onChanged({ start: period.start, end: period.end });
  };

  return (
    <div className="date-range-picker">
      {periods.map((period) => (
        <button
          key={period.label}
          type="button"
          className={`btn ${
            isSelected(period) ? 'btn-primary' : 'btn-outline-primary'
          }`}
          onClick={() => handlePeriodClick(period)}
        >
          {period.label}
        </button>
      ))}
    </div>
  );
};

export default DateRangePicker;
