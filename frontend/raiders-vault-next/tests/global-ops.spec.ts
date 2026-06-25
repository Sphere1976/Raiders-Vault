import { expect, test } from '@playwright/test';

test('renders global operations dashboard with live ops sections', async ({ page }) => {
  await page.goto('/global-ops');

  await expect(page.getByRole('heading', { name: 'Global Ops' })).toBeVisible();
  await expect(page.getByText('Live Banner Feed')).toBeVisible();
  await expect(page.getByText('News Feed')).toBeVisible();
});
