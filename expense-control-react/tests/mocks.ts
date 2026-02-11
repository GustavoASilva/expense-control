import { Page } from '@playwright/test';

// Mock Cognito auth session in localStorage so Amplify treats the user as authenticated.
// The key format matches what amazon-cognito-identity-js stores internally.
const MOCK_CLIENT_ID = 'YOUR_APP_CLIENT_ID';
const MOCK_USERNAME = 'testuser';

// Minimal JWT-like token for mock purposes (not a real token, just enough for Amplify to parse)
const mockIdToken = [
  btoa(JSON.stringify({ alg: 'RS256', kid: 'mock' })),
  btoa(JSON.stringify({
    sub: '00000000-0000-0000-0000-000000000001',
    'cognito:username': MOCK_USERNAME,
    email: 'test@example.com',
    exp: Math.floor(Date.now() / 1000) + 3600,
    iat: Math.floor(Date.now() / 1000),
    iss: 'https://cognito-idp.us-east-1.amazonaws.com/YOUR_USER_POOL_ID',
    aud: MOCK_CLIENT_ID,
    token_use: 'id',
  })),
  'mock-signature',
].join('.');

const mockAccessToken = [
  btoa(JSON.stringify({ alg: 'RS256', kid: 'mock' })),
  btoa(JSON.stringify({
    sub: '00000000-0000-0000-0000-000000000001',
    'cognito:username': MOCK_USERNAME,
    client_id: MOCK_CLIENT_ID,
    exp: Math.floor(Date.now() / 1000) + 3600,
    iat: Math.floor(Date.now() / 1000),
    iss: 'https://cognito-idp.us-east-1.amazonaws.com/YOUR_USER_POOL_ID',
    token_use: 'access',
    scope: 'openid profile email',
  })),
  'mock-signature',
].join('.');

export async function setupAuthMock(page: Page) {
  const prefix = `CognitoIdentityServiceProvider.${MOCK_CLIENT_ID}`;
  await page.addInitScript(({ prefix, username, idToken, accessToken }) => {
    localStorage.setItem(`${prefix}.LastAuthUser`, username);
    localStorage.setItem(`${prefix}.${username}.idToken`, idToken);
    localStorage.setItem(`${prefix}.${username}.accessToken`, accessToken);
    localStorage.setItem(`${prefix}.${username}.refreshToken`, 'mock-refresh-token');
    localStorage.setItem(`${prefix}.${username}.clockDrift`, '0');
  }, { prefix, username: MOCK_USERNAME, idToken: mockIdToken, accessToken: mockAccessToken });
}

export const mockCategories = [
  { id: 'cat-1', name: 'Groceries', type: 'Expense', iconName: 'cart' },
  { id: 'cat-2', name: 'Rent', type: 'Expense', iconName: 'house' },
  { id: 'cat-3', name: 'Salary', type: 'Income', iconName: 'cash' },
  { id: 'cat-4', name: 'Freelance', type: 'Income', iconName: 'briefcase' },
];

export const mockTransactions = [
  {
    id: 'txn-1',
    amount: 55.0,
    description: 'Weekly groceries',
    date: '2026-02-05',
    type: 'Expense',
    categoryId: 'cat-1',
    categoryName: 'Groceries',
  },
  {
    id: 'txn-2',
    amount: 1200.0,
    description: 'Monthly rent',
    date: '2026-02-01',
    type: 'Expense',
    categoryId: 'cat-2',
    categoryName: 'Rent',
  },
  {
    id: 'txn-3',
    amount: 5000.0,
    description: 'Monthly salary',
    date: '2026-02-01',
    type: 'Income',
    categoryId: 'cat-3',
    categoryName: 'Salary',
  },
];

export const mockBalance = {
  income: 5000.0,
  expenses: 1255.0,
  balance: 3745.0,
  periodStart: '2026-01-09',
  periodEnd: '2026-02-07',
  hasTransactions: true,
};

export const mockCategoryBalances = {
  categories: [
    { categoryName: 'Salary', transactionType: 'Income', total: 5000.0, count: 1 },
    { categoryName: 'Rent', transactionType: 'Expense', total: 1200.0, count: 1 },
    { categoryName: 'Groceries', transactionType: 'Expense', total: 55.0, count: 1 },
  ],
};

export const mockMonthlyBalance = {
  year: 2026,
  months: [
    { month: 1, monthName: 'January', income: 5000, expenses: 1100, balance: 3900, hasTransactions: true },
    { month: 2, monthName: 'February', income: 5000, expenses: 1255, balance: 3745, hasTransactions: true },
    { month: 3, monthName: 'March', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 4, monthName: 'April', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 5, monthName: 'May', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 6, monthName: 'June', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 7, monthName: 'July', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 8, monthName: 'August', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 9, monthName: 'September', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 10, monthName: 'October', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 11, monthName: 'November', income: 0, expenses: 0, balance: 0, hasTransactions: false },
    { month: 12, monthName: 'December', income: 0, expenses: 0, balance: 0, hasTransactions: false },
  ],
  hasTransactions: true,
  totalIncome: 10000,
  totalExpenses: 2355,
  yearlyBalance: 7645,
};

export const mockBudgets = [
  {
    id: 'bud-1',
    categoryId: 'cat-1',
    category: { id: 'cat-1', name: 'Groceries', type: 'Expense', iconName: 'cart' },
    amount: 300,
    month: 2,
    year: 2026,
    createdAt: '2026-02-01T00:00:00Z',
    updatedAt: '2026-02-01T00:00:00Z',
  },
  {
    id: 'bud-2',
    categoryId: 'cat-2',
    category: { id: 'cat-2', name: 'Rent', type: 'Expense', iconName: 'house' },
    amount: 1500,
    month: 2,
    year: 2026,
    createdAt: '2026-02-01T00:00:00Z',
    updatedAt: '2026-02-01T00:00:00Z',
  },
];

export const mockBudgetUsageGroceries = {
  amount: 300,
  usage: 55,
  percent: 18.33,
};

export const mockBudgetUsageRent = {
  amount: 1500,
  usage: 1200,
  percent: 80,
};

export async function setupApiMocks(page: Page) {
  // Set up auth mock so the app treats the user as authenticated
  await setupAuthMock(page);

  await page.route('**/api/categories', (route) => {
    route.fulfill({ json: mockCategories });
  });

  await page.route('**/api/transactions*', (route) => {
    if (route.request().method() === 'GET') {
      route.fulfill({ json: mockTransactions });
    } else if (route.request().method() === 'POST') {
      route.fulfill({
        json: { ...mockTransactions[0], id: 'txn-new' },
        status: 201,
      });
    } else if (route.request().method() === 'PATCH') {
      route.fulfill({ json: mockTransactions[0] });
    } else if (route.request().method() === 'DELETE') {
      route.fulfill({ status: 204 });
    } else {
      route.continue();
    }
  });

  await page.route('**/api/balance/by-category*', (route) => {
    route.fulfill({ json: mockCategoryBalances });
  });

  await page.route('**/api/balance/monthly*', (route) => {
    route.fulfill({ json: mockMonthlyBalance });
  });

  await page.route('**/api/balance*', (route) => {
    if (route.request().url().includes('/by-category') || route.request().url().includes('/monthly')) {
      return;
    }
    route.fulfill({ json: mockBalance });
  });

  await page.route('**/api/budgets/**', (route) => {
    const url = route.request().url();
    if (url.includes('/usage')) {
      if (url.includes('cat-1')) {
        route.fulfill({ json: mockBudgetUsageGroceries });
      } else if (url.includes('cat-2')) {
        route.fulfill({ json: mockBudgetUsageRent });
      } else {
        route.fulfill({ json: { amount: 0, usage: 0, percent: 0 } });
      }
    } else {
      route.continue();
    }
  });

  await page.route(/\/api\/budgets(\?|$)/, (route) => {
    if (route.request().method() === 'GET') {
      route.fulfill({ json: mockBudgets });
    } else if (route.request().method() === 'POST') {
      route.fulfill({
        json: { ...mockBudgets[0], id: 'bud-new' },
        status: 201,
      });
    } else {
      route.continue();
    }
  });
}
