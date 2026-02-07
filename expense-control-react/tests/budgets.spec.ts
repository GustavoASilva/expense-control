import { test, expect } from '@playwright/test';
import { setupApiMocks } from './mocks';

test.describe('Budgets', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/budgets');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Budgets' })).toBeVisible();
  });

  test('should display the New Budget button', async ({ page }) => {
    await expect(page.getByRole('button', { name: /New Budget/i })).toBeVisible();
  });

  test('should display month and year selectors', async ({ page }) => {
    await expect(page.getByRole('combobox').first()).toBeVisible();
    await expect(page.getByRole('combobox').nth(1)).toBeVisible();
  });

  test('should display budget cards with category names', async ({ page }) => {
    await expect(page.getByText('Groceries')).toBeVisible();
    await expect(page.getByText('Rent')).toBeVisible();
  });

  test('should display budget amounts', async ({ page }) => {
    const groceriesCard = page.locator('.card').filter({ hasText: 'Groceries' });
    await expect(groceriesCard.getByText('$300.00').first()).toBeVisible();
    const rentCard = page.locator('.card').filter({ hasText: 'Rent' });
    await expect(rentCard.getByText('$1,500.00').first()).toBeVisible();
  });

  test('should display budget usage percentages', async ({ page }) => {
    await expect(page.getByText('18%')).toBeVisible();
    await expect(page.getByText('80%')).toBeVisible();
  });

  test('should display budget status badges', async ({ page }) => {
    await expect(page.getByText('Under budget')).toBeVisible();
    await expect(page.getByText('Almost spent')).toBeVisible();
  });

  test('should display progress bars for each budget', async ({ page }) => {
    const progressBars = page.getByRole('progressbar');
    await expect(progressBars).toHaveCount(2);
  });

  test('should open the New Budget form when clicking the button', async ({ page }) => {
    await page.getByRole('button', { name: /New Budget/i }).click();
    await expect(page.getByText('New Budget', { exact: false }).last()).toBeVisible();
    await expect(page.getByLabel('Category')).toBeVisible();
    await expect(page.getByLabel('Budget Amount')).toBeVisible();
    await expect(page.getByLabel('Month')).toBeVisible();
    await expect(page.getByLabel('Year')).toBeVisible();
  });

  test('should close the budget form when clicking Cancel', async ({ page }) => {
    await page.getByRole('button', { name: /New Budget/i }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).not.toBeVisible();
  });

  test('should show validation errors for empty budget form submission', async ({ page }) => {
    await page.getByRole('button', { name: /New Budget/i }).click();
    await page.locator('.modal').getByRole('button', { name: /Create Budget/i }).click();

    await expect(page.getByText('Amount must be greater than 0')).toBeVisible();
    await expect(page.getByText('Please select a category')).toBeVisible();
  });

  test('should show empty state when no budgets exist', async ({ page }) => {
    await page.route(/\/api\/budgets(\?|$)/, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({ json: [] });
      }
    });
    await page.goto('/budgets');
    await expect(page.getByText('No budgets found')).toBeVisible();
    await expect(page.getByRole('button', { name: /Create Budget/i })).toBeVisible();
  });

  test('should open edit form when clicking a budget card', async ({ page }) => {
    await page.locator('.card').filter({ hasText: 'Groceries' }).click();
    await expect(page.getByText('Edit Budget')).toBeVisible();
    await expect(page.getByRole('button', { name: /Save Changes/i })).toBeVisible();
  });
});
