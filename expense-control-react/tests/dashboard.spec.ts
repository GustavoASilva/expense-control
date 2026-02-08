import { test, expect } from '@playwright/test';
import { setupApiMocks } from './mocks';

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  test('should display stat cards for Income, Expenses, and Balance', async ({ page }) => {
    await expect(page.getByText('Income', { exact: true }).first()).toBeVisible();
    await expect(page.getByText('Expenses', { exact: true }).first()).toBeVisible();
    await expect(page.getByText('Balance', { exact: true }).first()).toBeVisible();
  });

  test('should display formatted currency values in stat cards', async ({ page }) => {
    const statCards = page.locator('.stat-card');
    await expect(statCards.filter({ hasText: 'Income' }).getByText('$5,000.00')).toBeVisible();
    await expect(statCards.filter({ hasText: 'Expenses' }).getByText('$1,255.00')).toBeVisible();
    await expect(statCards.filter({ hasText: 'Balance' }).getByText('$3,745.00')).toBeVisible();
  });

  test('should display the Monthly Overview chart section', async ({ page }) => {
    await expect(page.getByText(/Monthly Overview/)).toBeVisible();
  });

  test('should display the Top Categories section', async ({ page }) => {
    await expect(page.getByText('Top Categories')).toBeVisible();
    const topCategoriesCard = page.locator('.card').filter({ hasText: 'Top Categories' });
    await expect(topCategoriesCard.getByText('Salary')).toBeVisible();
    await expect(topCategoriesCard.getByText('Rent')).toBeVisible();
    await expect(topCategoriesCard.getByText('Groceries')).toBeVisible();
  });

  test('should display recent transactions', async ({ page }) => {
    await expect(page.getByText('Recent Transactions')).toBeVisible();
    await expect(page.getByText('Weekly groceries')).toBeVisible();
    await expect(page.getByText('Monthly rent')).toBeVisible();
    await expect(page.getByText('Monthly salary')).toBeVisible();
  });

  test('should display date range picker buttons', async ({ page }) => {
    await expect(page.getByRole('button', { name: '7 days' })).toBeVisible();
    await expect(page.getByRole('button', { name: '30 days' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'This month' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Last month' })).toBeVisible();
    await expect(page.getByRole('button', { name: '3 months' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'This year' })).toBeVisible();
  });

  test('should have a View All link to transactions page', async ({ page }) => {
    const viewAllLink = page.getByRole('link', { name: /View All/i });
    await expect(viewAllLink).toBeVisible();
    await viewAllLink.click();
    await expect(page).toHaveURL('/transactions');
  });

  test('should show empty state when there are no transactions', async ({ page }) => {
    await page.route('**/api/transactions*', (route) => {
      route.fulfill({ json: [] });
    });
    await page.route('**/api/balance*', (route) => {
      if (route.request().url().includes('/by-category') || route.request().url().includes('/monthly')) {
        return;
      }
      route.fulfill({
        json: { income: 0, expenses: 0, balance: 0, periodStart: '', periodEnd: '', hasTransactions: false },
      });
    });
    await page.route('**/api/balance/by-category*', (route) => {
      route.fulfill({ json: { categories: [] } });
    });
    await page.route('**/api/balance/monthly*', (route) => {
      route.fulfill({
        json: { ...{ year: 2026, months: [], hasTransactions: false, totalIncome: 0, totalExpenses: 0, yearlyBalance: 0 } },
      });
    });
    await page.goto('/');
    await expect(page.getByText('No transactions yet')).toBeVisible();
  });
});
