import { test, expect } from '@playwright/test';
import { setupAuthMock, mockHousehold } from './mocks';

test.describe('Household Setup', () => {
  test('should redirect to household setup when user has no household', async ({ page }) => {
    await setupAuthMock(page);

    await page.route('**/api/users/me/household', (route) => {
      route.fulfill({ json: { household: null } });
    });

    await page.goto('/');
    await expect(page).toHaveURL('/household/setup');
  });

  test('should display household setup form', async ({ page }) => {
    await setupAuthMock(page);

    await page.route('**/api/users/me/household', (route) => {
      route.fulfill({ json: { household: null } });
    });

    await page.goto('/household/setup');

    await expect(page.getByRole('heading', { name: 'Set Up Your Household' })).toBeVisible();
    await expect(page.getByText('Create a household to get started')).toBeVisible();
    await expect(page.getByLabel('Household Name')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Create Household' })).toBeVisible();
  });

  test('should create household and redirect to dashboard', async ({ page }) => {
    await setupAuthMock(page);

    let householdCreated = false;

    await page.route('**/api/users/me/household', (route) => {
      if (householdCreated) {
        route.fulfill({ json: { household: mockHousehold } });
      } else {
        route.fulfill({ json: { household: null } });
      }
    });

    await page.route('**/api/households', (route) => {
      if (route.request().method() === 'POST') {
        householdCreated = true;
        route.fulfill({ json: mockHousehold, status: 201 });
      } else {
        route.continue();
      }
    });

    // Mock remaining API endpoints for the dashboard
    await page.route('**/api/balance/by-category*', (route) => {
      route.fulfill({ json: { categories: [] } });
    });
    await page.route('**/api/balance/monthly*', (route) => {
      route.fulfill({ json: { year: 2026, months: [], hasTransactions: false, totalIncome: 0, totalExpenses: 0, yearlyBalance: 0 } });
    });
    await page.route('**/api/balance*', (route) => {
      if (route.request().url().includes('/by-category') || route.request().url().includes('/monthly')) {
        return;
      }
      route.fulfill({ json: { income: 0, expenses: 0, balance: 0, periodStart: '2026-01-01', periodEnd: '2026-02-01', hasTransactions: false } });
    });
    await page.route('**/api/transactions*', (route) => {
      route.fulfill({ json: [] });
    });

    await page.goto('/household/setup');

    await page.getByLabel('Household Name').fill('My Family');
    await page.getByRole('button', { name: 'Create Household' }).click();

    await expect(page).toHaveURL('/');
  });

  test('should show error when creation fails', async ({ page }) => {
    await setupAuthMock(page);

    await page.route('**/api/users/me/household', (route) => {
      route.fulfill({ json: { household: null } });
    });

    await page.route('**/api/households', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({ status: 500, json: { error: 'Internal error' } });
      } else {
        route.continue();
      }
    });

    await page.goto('/household/setup');

    await page.getByLabel('Household Name').fill('My Family');
    await page.getByRole('button', { name: 'Create Household' }).click();

    await expect(page.getByText('Failed to create household. Please try again.')).toBeVisible();
  });
});
