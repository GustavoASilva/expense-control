import { test, expect } from '@playwright/test';
import { setupApiMocks } from './mocks';

test.describe('Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
  });

  test('should display the application brand in the sidebar', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('link', { name: /ExpenseCtrl/i })).toBeVisible();
  });

  test('should display navigation links for all pages', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('link', { name: 'Dashboard' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Transactions' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Budgets' })).toBeVisible();
  });

  test('should navigate to Dashboard page', async ({ page }) => {
    await page.goto('/transactions');
    await page.getByRole('link', { name: 'Dashboard' }).click();
    await expect(page).toHaveURL('/');
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  test('should navigate to Transactions page', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: 'Transactions' }).click();
    await expect(page).toHaveURL('/transactions');
    await expect(page.getByRole('heading', { name: 'Transactions' })).toBeVisible();
  });

  test('should navigate to Budgets page', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: 'Budgets' }).click();
    await expect(page).toHaveURL('/budgets');
    await expect(page.getByRole('heading', { name: 'Budgets' })).toBeVisible();
  });

  test('should highlight active navigation link', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('link', { name: 'Dashboard' })).toHaveClass(/active/);

    await page.getByRole('link', { name: 'Transactions' }).click();
    await expect(page.getByRole('link', { name: 'Transactions' })).toHaveClass(/active/);

    await page.getByRole('link', { name: 'Budgets' }).click();
    await expect(page.getByRole('link', { name: 'Budgets' })).toHaveClass(/active/);
  });
});
