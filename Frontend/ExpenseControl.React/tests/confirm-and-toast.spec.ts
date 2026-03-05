import { test, expect } from '@playwright/test';
import { setupApiMocks } from './mocks';

test.describe('Confirmation Dialog and Toast Notifications', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
  });

  test('should show confirmation dialog when deleting a transaction', async ({ page }) => {
    await page.goto('/transactions');

    await test.step('Click delete button on a transaction', async () => {
      await page.locator('button[title="Delete"]').first().click();
    });

    await test.step('Verify confirmation dialog is visible', async () => {
      await expect(page.getByText('Delete Transaction')).toBeVisible();
      await expect(page.getByText(/Are you sure you want to delete/)).toBeVisible();
      await expect(page.getByRole('button', { name: 'Delete' })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    });
  });

  test('should close confirmation dialog when clicking Cancel', async ({ page }) => {
    await page.goto('/transactions');

    await test.step('Open confirmation dialog', async () => {
      await page.locator('button[title="Delete"]').first().click();
      await expect(page.getByText('Delete Transaction')).toBeVisible();
    });

    await test.step('Click Cancel to dismiss', async () => {
      await page.locator('.modal-footer').getByRole('button', { name: 'Cancel' }).click();
      await expect(page.getByText('Delete Transaction')).not.toBeVisible();
    });
  });

  test('should delete transaction when confirming in dialog', async ({ page }) => {
    await page.goto('/transactions');

    await test.step('Open and confirm deletion', async () => {
      await page.locator('button[title="Delete"]').first().click();
      await expect(page.getByText('Delete Transaction')).toBeVisible();
      await page.getByRole('button', { name: 'Delete' }).click();
    });

    await test.step('Verify confirmation dialog closes', async () => {
      await expect(page.getByText('Delete Transaction')).not.toBeVisible();
    });
  });

  test('should show error toast when transaction save fails', async ({ page }) => {
    await page.route('**/api/transactions*', (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({ json: [] });
      } else if (route.request().method() === 'POST') {
        route.fulfill({ status: 500, json: { error: 'Internal Server Error' } });
      } else {
        route.continue();
      }
    });

    await page.goto('/transactions');

    await test.step('Open new transaction form and submit', async () => {
      await page.getByRole('button', { name: /Create Transaction/i }).click();
      await page.getByLabel('Amount').fill('100');
      await page.getByLabel('Description').fill('Test transaction');
      await page.getByLabel('Category').selectOption({ index: 1 });
      await page.locator('.modal').getByRole('button', { name: /Create Transaction/i }).click();
    });

    await test.step('Verify error toast appears', async () => {
      await expect(page.getByRole('alert').filter({ hasText: 'Failed to save transaction' })).toBeVisible();
    });
  });

  test('should show error toast when budget save fails', async ({ page }) => {
    await page.route(/\/api\/budgets(\?|$)/, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({ json: [] });
      } else if (route.request().method() === 'POST') {
        route.fulfill({ status: 500, json: { error: 'Internal Server Error' } });
      } else {
        route.continue();
      }
    });

    await page.goto('/budgets');

    await test.step('Open new budget form and submit', async () => {
      await page.getByRole('button', { name: /Create Budget/i }).click();
      await page.getByLabel('Category').selectOption({ index: 1 });
      await page.getByLabel('Budget Amount').fill('500');
      await page.locator('.modal').getByRole('button', { name: /Create Budget/i }).click();
    });

    await test.step('Verify error toast appears', async () => {
      await expect(page.getByRole('alert').filter({ hasText: 'Failed to save budget' })).toBeVisible();
    });
  });

  test('should show error toast when transaction delete fails', async ({ page }) => {
    await page.route('**/api/transactions*', (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({
          json: [
            {
              id: 'txn-1',
              amount: 55.0,
              description: 'Weekly groceries',
              date: '2026-02-05',
              type: 'Expense',
              categoryId: 'cat-1',
              categoryName: 'Groceries',
            },
          ],
        });
      } else if (route.request().method() === 'DELETE') {
        route.fulfill({ status: 500, json: { error: 'Internal Server Error' } });
      } else {
        route.continue();
      }
    });

    await page.goto('/transactions');

    await test.step('Click delete and confirm', async () => {
      await page.locator('button[title="Delete"]').first().click();
      await expect(page.getByText('Delete Transaction')).toBeVisible();
      await page.getByRole('button', { name: 'Delete' }).click();
    });

    await test.step('Verify error toast appears', async () => {
      await expect(page.getByRole('alert').filter({ hasText: 'Failed to delete transaction' })).toBeVisible();
    });
  });
});
