import { test, expect } from '@playwright/test';
import { setupApiMocks } from './mocks';

test.describe('Categories', () => {
  test.beforeEach(async ({ page }) => {
    await setupApiMocks(page);
    await page.goto('/categories');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Categories' })).toBeVisible();
  });

  test('should display the New Category button', async ({ page }) => {
    await expect(page.getByRole('button', { name: /New Category/i })).toBeVisible();
  });

  test('should display expense and income category sections', async ({ page }) => {
    await expect(page.getByText('Expense Categories')).toBeVisible();
    await expect(page.getByText('Income Categories')).toBeVisible();
  });

  test('should display expense categories', async ({ page }) => {
    await expect(page.getByText('Groceries')).toBeVisible();
    await expect(page.getByText('Rent')).toBeVisible();
  });

  test('should display income categories', async ({ page }) => {
    await expect(page.getByText('Salary')).toBeVisible();
    await expect(page.getByText('Freelance')).toBeVisible();
  });

  test('should open the New Category form when clicking the button', async ({ page }) => {
    await page.getByRole('button', { name: /New Category/i }).click();

    await expect(page.getByText('New Category', { exact: false }).last()).toBeVisible();
    await expect(page.getByLabel('Name', { exact: true })).toBeVisible();
  });

  test('should close the category form when clicking Cancel', async ({ page }) => {
    await page.getByRole('button', { name: /New Category/i }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByRole('button', { name: 'Cancel' })).not.toBeVisible();
  });

  test('should show validation error when name is empty', async ({ page }) => {
    await page.getByRole('button', { name: /New Category/i }).click();
    await page.locator('.modal').getByRole('button', { name: /Create Category/i }).click();

    await expect(page.getByText('Category name is required.')).toBeVisible();
  });

  test('should toggle between Expense and Income types in form', async ({ page }) => {
    await page.getByRole('button', { name: /New Category/i }).click();

    await test.step('Default type is Expense', async () => {
      await expect(page.locator('#expenseCatType')).toBeChecked();
    });

    await test.step('Switch to Income', async () => {
      await page.locator('label[for="incomeCatType"]').click();
      await expect(page.locator('#incomeCatType')).toBeChecked();
    });
  });

  test('should create a new category successfully', async ({ page }) => {
    await page.getByRole('button', { name: /New Category/i }).click();

    await test.step('Fill in the form', async () => {
      await page.getByLabel('Name', { exact: true }).fill('Entertainment');
    });

    await test.step('Submit the form', async () => {
      await page.locator('.modal').getByRole('button', { name: /Create Category/i }).click();
    });

    await test.step('Verify form closes', async () => {
      await expect(page.locator('.modal')).not.toBeVisible();
    });
  });

  test('should display full-screen form modal on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.getByRole('button', { name: /New Category/i }).click();

    await test.step('Verify form header is visible', async () => {
      await expect(page.getByText('New Category', { exact: false }).last()).toBeVisible();
    });

    await test.step('Verify form fields are accessible', async () => {
      await expect(page.getByLabel('Name', { exact: true })).toBeVisible();
    });

    await test.step('Verify action buttons are reachable', async () => {
      await expect(page.locator('.form-modal').getByRole('button', { name: /Create Category/i })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();
    });
  });
});
