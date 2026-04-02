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
  category?: Category;
  categoryName: string;
  notes?: string;
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
  isDefault: boolean;
}

export interface Balance {
  income: number;
  expenses: number;
  balance: number;
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

export interface Budget {
  id: string;
  categoryId: string;
  category: Category;
  amount: number;
  month: number;
  year: number;
  createdAt: string;
  updatedAt: string;
}

export interface BudgetForm {
  categoryId: string;
  amount: number;
  month: number;
  year: number;
}

export interface BudgetUsage {
  amount: number;
  usage: number;
  percent: number;
}

export interface Saving {
  id: string;
  name: string;
  description?: string;
  currentAmount: number;
  targetAmount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface SavingForm {
  name: string;
  description?: string;
  currentAmount: number;
  targetAmount?: number;
}

export interface Household {
  id: string;
  name: string;
  inviteId: string;
  createdAt: string;
}

export interface UserHouseholdResponse {
  household: Household | null;
}

export interface CategoryForm {
  name: string;
  description?: string;
  type: TransactionType;
  iconName?: string;
}
