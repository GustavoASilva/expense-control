import axios from 'axios';
import { fetchAuthSession } from 'aws-amplify/auth';
import {
  Transaction,
  TransactionForm,
  Category,
  CategoryForm,
  Balance,
  CategoryBalance,
  MonthlyBalance,
  Budget,
  BudgetForm,
  BudgetUsage,
  Household,
  UserHouseholdResponse,
} from '../types/index';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5293/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Attach Cognito access token to every request
api.interceptors.request.use(async (config) => {
  // Check for mock auth
  if (import.meta.env.VITE_USE_MOCK_AUTH === 'true') {
    config.headers.Authorization = 'Bearer mock-access-token';
    return config;
  }

  try {
    const session = await fetchAuthSession();
    const token = session.tokens?.accessToken?.toString();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  } catch {
    // If session fetch fails, proceed without token
  }
  return config;
});

// Redirect to login on 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Helper to format dates for API
const formatDate = (date: string | Date): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toISOString().split('T')[0];
};

// Transactions
export const getTransactions = async (
  startDate?: string,
  endDate?: string
): Promise<Transaction[]> => {
  const params: Record<string, string> = {};
  if (startDate) params.startDate = formatDate(startDate);
  if (endDate) params.endDate = formatDate(endDate);

  const response = await api.get<Transaction[]>('/transactions', { params });
  return response.data;
};

export const getTransaction = async (id: string): Promise<Transaction> => {
  const response = await api.get<Transaction>(`/transactions/${id}`);
  return response.data;
};

export const createTransaction = async (
  transaction: TransactionForm
): Promise<Transaction> => {
  const response = await api.post<Transaction>('/transactions', transaction);
  return response.data;
};

export const updateTransaction = async (
  id: string,
  transaction: TransactionForm
): Promise<Transaction> => {
  const response = await api.patch<Transaction>(`/transactions/${id}`, transaction);
  return response.data;
};

export const deleteTransaction = async (id: string): Promise<void> => {
  await api.delete(`/transactions/${id}`);
};

// Categories
export const getCategories = async (): Promise<Category[]> => {
  const response = await api.get<Category[]>('/categories');
  return response.data;
};

export const createCategory = async (category: CategoryForm): Promise<Category> => {
  const response = await api.post<Category>('/categories', category);
  return response.data;
};

// Balance
export const getBalance = async (
  startDate?: string,
  endDate?: string
): Promise<Balance> => {
  const params: Record<string, string> = {};
  if (startDate) params.startDate = formatDate(startDate);
  if (endDate) params.endDate = formatDate(endDate);

  const response = await api.get<Balance>('/balance', { params });
  return response.data;
};

export const getBalanceByCategory = async (
  startDate?: string,
  endDate?: string
): Promise<CategoryBalance[]> => {
  const params: Record<string, string> = {};
  if (startDate) params.startDate = formatDate(startDate);
  if (endDate) params.endDate = formatDate(endDate);

  const response = await api.get<{ categories: CategoryBalance[] }>(
    '/balance/by-category',
    { params }
  );
  return response.data.categories;
};

export const getMonthlyBalance = async (year?: number): Promise<MonthlyBalance> => {
  const params: Record<string, number> = {};
  if (year) params.year = year;

  const response = await api.get<MonthlyBalance>('/balance/monthly', { params });
  return response.data;
};

// Budgets
export const getBudgets = async (
  year?: number,
  month?: number,
  categoryId?: string
): Promise<Budget[]> => {
  const params: Record<string, string | number> = {};
  if (year) params.year = year;
  if (month) params.month = month;
  if (categoryId) params.categoryId = categoryId;

  const response = await api.get<Budget[]>('/budgets', { params });
  return response.data;
};

export const createBudget = async (budget: BudgetForm): Promise<Budget> => {
  const response = await api.post<Budget>('/budgets', budget);
  return response.data;
};

export const getBudgetUsage = async (
  categoryId: string,
  year: number,
  month: number
): Promise<BudgetUsage> => {
  const params = { categoryId, year, month };
  const response = await api.get<BudgetUsage>('/budgets/usage', { params });
  return response.data;
};

// Households
export const getUserHousehold = async (): Promise<UserHouseholdResponse> => {
  const response = await api.get<UserHouseholdResponse>('/users/me/household');
  return response.data;
};

export const createHousehold = async (name: string): Promise<Household> => {
  const response = await api.post<Household>('/households', { name });
  return response.data;
};

export const joinHousehold = async (inviteId: string): Promise<Household> => {
  const response = await api.post<Household>('/households/join', { inviteId });
  return response.data;
};
