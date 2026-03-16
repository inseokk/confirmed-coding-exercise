import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import App from './App';

const mockRepos = [
  { Name: 'repo1', Description: 'D', Stars: 1, PrimaryLanguage: 'C#', Url: 'https://github.com/u/repo1', LastUpdated: '2025-01-01' },
];

const originalFetch = globalThis.fetch;

describe('App', () => {
  beforeEach(() => {
    globalThis.fetch = vi.fn();
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('shows loading state initially', () => {
    vi.mocked(fetch).mockImplementation(() => new Promise(() => {}));
    render(<App />);
    expect(screen.getByText(/loading repositories/i)).toBeInTheDocument();
  });

  it('shows repos when fetch succeeds', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockRepos),
    } as Response);
    render(<App />);
    await waitFor(() => {
      expect(screen.getByText('Top repositories')).toBeInTheDocument();
    });
    expect(screen.getByText('repo1')).toBeInTheDocument();
  });

  it('shows error when fetch fails after retries', async () => {
    vi.mocked(fetch).mockRejectedValue(new Error('Network error'));
    render(<App />);
    await waitFor(
      () => {
        expect(screen.getByText(/network error/i)).toBeInTheDocument();
      },
      { timeout: 5000 }
    );
  }, 6000);

  it('shows error when response is not ok', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
    } as Response);
    render(<App />);
    await waitFor(
      () => {
        expect(screen.getByText(/failed to load|404|not found/i)).toBeInTheDocument();
      },
      { timeout: 10000 }
    );
  }, 12000);
});
