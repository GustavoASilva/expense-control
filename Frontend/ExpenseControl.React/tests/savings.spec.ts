import { test, expect } from '@playwright/test';
import { setupApiMocks, mockSavings } from './mocks';

test.describe('Savings', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/savings');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Savings' })).toBeVisible();
  });

  test('should display the New Savings Fund button', async ({ page }) => {
    await expect(page.getByRole('button', { name: /New Savings Fund/i })).toBeVisible();
  });

  test('should display savings fund cards with names', async ({ page }) => {
    await expect(page.getByText('Emergency Fund')).toBeVisible();
    await expect(page.getByText('Vacation Fund')).toBeVisible();
  });

  test('should display savings fund descriptions', async ({ page }) => {
    await expect(page.getByText('3 months of expenses')).toBeVisible();
  });

  test('should display current amounts for each fund', async ({ page }) => {
    await expect(page.getByText('$3,000.00').first()).toBeVisible();
    await expect(page.getByText('$1,500.00').first()).toBeVisible();
  });

  test('should display progress bars for funds with targets', async ({ page }) => {
    const progressBars = page.getByRole('progressbar');
    await expect(progressBars).toHaveCount(2);
  });

  test('should display progress percentages for funds with targets', async ({ page }) => {
    await expect(page.getByText('30%')).toBeVisible();
    await expect(page.getByText('75%')).toBeVisible();
  });

  test('should display goal amounts for funds with targets', async ({ page }) => {
    await expect(page.getByText('$10,000.00 goal')).toBeVisible();
    await expect(page.getByText('$2,000.00 goal')).toBeVisible();
  });

  test('should display summary cards with totals', async ({ page }) => {
    await expect(page.getByText('Total Saved')).toBeVisible();
    await expect(page.getByText('$4,500.00')).toBeVisible();
    await expect(page.getByText('Total Target')).toBeVisible();
    await expect(page.getByText('$12,000.00')).toBeVisible();
    await expect(page.getByText('Funds')).toBeVisible();
    await expect(page.getByText('2').first()).toBeVisible();
  });

  test('should display edit and delete buttons on each fund card', async ({ page }) => {
    const editButtons = page.locator('button[title="Edit"]');
    const deleteButtons = page.locator('button[title="Delete"]');
    await expect(editButtons).toHaveCount(2);
    await expect(deleteButtons).toHaveCount(2);
  });

  test('should open the New Savings Fund form when clicking the button', async ({ page }) => {
    await page.getByRole('button', { name: /New Savings Fund/i }).click();

    await expect(page.getByRole('heading', { name: 'New Savings Fund' })).toBeVisible();
    await expect(page.getByLabel('Name')).toBeVisible();
    await expect(page.getByLabel('Current Amount')).toBeVisible();
    await expect(page.getByLabel(/Target Amount/)).toBeVisible();
  });

  test('should close the form when clicking Cancel', async ({ page }) => {
    await page.getByRole('button', { name: /New Savings Fund/i }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).not.toBeVisible();
  });

  test('should show validation error when name is empty', async ({ page }) => {
    await page.getByRole('button', { name: /New Savings Fund/i }).click();
    await page.locator('.modal').getByRole('button', { name: /Create Savings Fund/i }).click();

    await expect(page.getByText('Name is required')).toBeVisible();
  });

  test('should create a new savings fund successfully', async ({ page }) => {
    await page.getByRole('button', { name: /New Savings Fund/i }).click();

    await test.step('Fill in the form', async () => {
      await page.getByLabel('Name').fill('New Emergency Fund');
      await page.getByLabel('Current Amount').fill('500');
      await page.getByLabel(/Target Amount/).fill('5000');
    });

    await test.step('Submit the form', async () => {
      await page.locator('.modal').getByRole('button', { name: /Create Savings Fund/i }).click();
    });

    await test.step('Verify form closes', async () => {
      await expect(page.locator('.modal')).not.toBeVisible();
    });
  });

  test('should open edit form when clicking edit button on a fund card', async ({ page }) => {
    await page.locator('button[title="Edit"]').first().click();
    await expect(page.getByText('Edit Savings Fund')).toBeVisible();
    await expect(page.getByRole('button', { name: /Save Changes/i })).toBeVisible();
  });

  test('should pre-populate the edit form with fund data', async ({ page }) => {
    await page.locator('.card').filter({ hasText: 'Emergency Fund' }).locator('button[title="Edit"]').click();

    await expect(page.getByLabel('Name')).toHaveValue(mockSavings[0].name);
    await expect(page.getByLabel('Current Amount')).toHaveValue(String(mockSavings[0].currentAmount));
  });

  test('should open edit form when clicking a fund card', async ({ page }) => {
    await page.locator('.card').filter({ hasText: 'Emergency Fund' }).click();
    await expect(page.getByText('Edit Savings Fund')).toBeVisible();
  });

  test('should show delete confirmation dialog when clicking delete', async ({ page }) => {
    await page.locator('button[title="Delete"]').first().click();

    await expect(page.getByText('Delete Savings Fund')).toBeVisible();
    await expect(page.getByText(/Are you sure you want to delete/)).toBeVisible();
    await expect(page.getByRole('button', { name: 'Delete' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
  });

  test('should cancel deletion when clicking Cancel in the confirmation dialog', async ({ page }) => {
    await page.locator('button[title="Delete"]').first().click();
    await expect(page.getByText('Delete Savings Fund')).toBeVisible();

    await page.locator('.modal-footer').getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByText('Delete Savings Fund')).not.toBeVisible();
    await expect(page.getByText('Emergency Fund')).toBeVisible();
  });

  test('should delete a savings fund when confirming in the dialog', async ({ page }) => {
    await page.locator('button[title="Delete"]').first().click();
    await expect(page.getByText('Delete Savings Fund')).toBeVisible();

    await page.getByRole('button', { name: 'Delete' }).click();

    await expect(page.getByText('Delete Savings Fund')).not.toBeVisible();
  });

  test('should show empty state when no savings funds exist', async ({ page }) => {
    await page.route(/\/api\/savings(\?|$)/, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({ json: [] });
      }
    });
    await page.goto('/savings');

    await expect(page.getByText('No savings funds yet')).toBeVisible();
    await expect(page.getByRole('button', { name: /Create Savings Fund/i })).toBeVisible();
  });

  test('should show error toast when savings fund save fails', async ({ page }) => {
    await page.route(/\/api\/savings(\?|$)/, (route) => {
      if (route.request().method() === 'GET') {
        route.fulfill({ json: [] });
      } else if (route.request().method() === 'POST') {
        route.fulfill({ status: 500, json: { error: 'Internal Server Error' } });
      } else {
        route.continue();
      }
    });
    await page.goto('/savings');

    await test.step('Open form and submit', async () => {
      await page.getByRole('button', { name: /New Savings Fund/i }).click();
      await page.getByLabel('Name').fill('Test Fund');
      await page.locator('.modal').getByRole('button', { name: /Create Savings Fund/i }).click();
    });

    await test.step('Verify error toast appears', async () => {
      await expect(page.getByRole('alert').filter({ hasText: 'Failed to save' })).toBeVisible();
    });
  });
});
