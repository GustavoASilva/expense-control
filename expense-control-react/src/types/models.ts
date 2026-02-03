export enum TransactionType {
  Expense = 'Expense',
  Income = 'Income',
}

export interface Transaction {
  id: string;
  amount: number;
  description: string;
  date: string;
  type: TransactionType;
  categoryId: string;
  categoryName: string;
}

export interface TransactionForm {
  id?: string;
  amount: number;
  description: string;
  date: string;
  type: TransactionType;
  categoryId: string;
  notes?: string;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  type: TransactionType;
  iconName: string;
}

export interface Balance {
  income: number;
  expenses: number;
  currentBalance: number;
  periodStart: string;
  periodEnd: string;
  hasTransactions: boolean;
}

export interface CategoryBalance {
  categoryName: string;
  transactionType: TransactionType;
  total: number;
  count: number;
}

export interface MonthSummary {
  month: number;
  monthName: string;
  income: number;
  expenses: number;
  balance: number;
  hasTransactions: boolean;
}

export interface MonthlyBalance {
  year: number;
  months: MonthSummary[];
  hasTransactions: boolean;
  totalIncome: number;
  totalExpenses: number;
  yearlyBalance: number;
}
