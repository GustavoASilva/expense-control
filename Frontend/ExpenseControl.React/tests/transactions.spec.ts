import { test, expect } from '@playwright/test';
import { setupApiMocks, mockTransactions } from './mocks';

test.describe('Transactions', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/transactions');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Transactions' })).toBeVisible();
  });

  test('should display the New Transaction button', async ({ page }) => {
    await expect(page.getByRole('button', { name: /New Transaction/i })).toBeVisible();
  });

  test('should display transaction list with all transactions', async ({ page }) => {
    for (const txn of mockTransactions) {
      await expect(page.getByText(txn.description)).toBeVisible();
    }
  });

  test('should display transaction amounts with currency formatting', async ({ page }) => {
    await expect(page.getByText('$55.00')).toBeVisible();
    await expect(page.getByText('$1,200.00')).toBeVisible();
    await expect(page.getByText('$5,000.00')).toBeVisible();
  });

  test('should display category badges for transactions', async ({ page }) => {
    await expect(page.locator('.category-badge').filter({ hasText: 'Groceries' })).toBeVisible();
    await expect(page.locator('.category-badge').filter({ hasText: 'Rent' })).toBeVisible();
    await expect(page.locator('.category-badge').filter({ hasText: 'Salary' })).toBeVisible();
  });

  test('should display date range picker', async ({ page }) => {
    await expect(page.getByRole('button', { name: '7 days' })).toBeVisible();
    await expect(page.getByRole('button', { name: '30 days' })).toBeVisible();
  });

  test('should open the New Transaction form when clicking the button', async ({ page }) => {
    await page.getByRole('button', { name: /New Transaction/i }).click();
    await expect(page.getByText('New Transaction', { exact: false }).last()).toBeVisible();
    await expect(page.getByLabel('Amount')).toBeVisible();
    await expect(page.getByLabel('Date')).toBeVisible();
    await expect(page.getByLabel('Description')).toBeVisible();
    await expect(page.getByLabel('Category')).toBeVisible();
  });

  test('should close the transaction form when clicking Cancel', async ({ page }) => {
    await page.getByRole('button', { name: /New Transaction/i }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).not.toBeVisible();
  });

  test('should toggle between Expense and Income types in form', async ({ page }) => {
    await page.getByRole('button', { name: /New Transaction/i }).click();

    await test.step('Default type is Expense', async () => {
      await expect(page.locator('#expenseTypeBtn')).toBeChecked();
    });

    await test.step('Switch to Income', async () => {
      await page.locator('label[for="incomeTypeBtn"]').click();
      await expect(page.locator('#incomeTypeBtn')).toBeChecked();
    });

    await test.step('Switch back to Expense', async () => {
      await page.locator('label[for="expenseTypeBtn"]').click();
      await expect(page.locator('#expenseTypeBtn')).toBeChecked();
    });
  });

  test('should show validation errors for empty form submission', async ({ page }) => {
    await page.getByRole('button', { name: /New Transaction/i }).click();
    await page.getByRole('button', { name: /Create Transaction/i }).click();

    await expect(page.getByText('Amount must be greater than 0')).toBeVisible();
    await expect(page.getByText('Description must be at least 3 characters')).toBeVisible();
    await expect(page.getByText('Please select a category')).toBeVisible();
  });

  test('should show empty state when no transactions exist', async ({ page }) => {
    await page.route('**/api/transactions*', (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({ json: [] });
      }
    });
    await page.goto('/transactions');
    await expect(page.getByText('No transactions found')).toBeVisible();
    await expect(page.getByRole('button', { name: /Create Transaction/i })).toBeVisible();
  });

  test('should open edit form when clicking a transaction edit button', async ({ page }) => {
    await page.locator('button[title="Edit"]').first().click();
    await expect(page.getByText('Edit Transaction')).toBeVisible();
    await expect(page.getByRole('button', { name: /Save Changes/i })).toBeVisible();
  });

  test('should display full-screen form modal on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.getByRole('button', { name: /New Transaction/i }).click();

    await test.step('Verify form header is visible', async () => {
      await expect(page.getByText('New Transaction', { exact: false }).last()).toBeVisible();
    });

    await test.step('Verify form fields are accessible', async () => {
      await expect(page.getByLabel('Amount')).toBeVisible();
      await expect(page.getByLabel('Description')).toBeVisible();
    });

    await test.step('Verify action buttons are reachable', async () => {
      await expect(page.getByRole('button', { name: /Create Transaction/i })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    });
  });
});
