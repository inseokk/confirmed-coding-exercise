import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect } from 'vitest';
import { RepoList } from './RepoList';
import type { RepositorySummary } from '../types/repo';

const mockRepos: RepositorySummary[] = [
  { Name: 'alpha', Description: 'First repo', Stars: 10, PrimaryLanguage: 'C#', Url: 'https://github.com/u/alpha', LastUpdated: '2025-01-01' },
  { Name: 'beta', Description: 'Second repo', Stars: 5, PrimaryLanguage: 'TS', Url: 'https://github.com/u/beta', LastUpdated: '2025-01-02' },
  { Name: 'gamma', Description: 'Third repo', Stars: 20, PrimaryLanguage: 'Rust', Url: 'https://github.com/u/gamma', LastUpdated: '2025-01-03' },
];

describe('RepoList', () => {
  it('renders all repos by default', () => {
    render(<RepoList repos={mockRepos} />);
    expect(screen.getByText('alpha')).toBeInTheDocument();
    expect(screen.getByText('beta')).toBeInTheDocument();
    expect(screen.getByText('gamma')).toBeInTheDocument();
  });

  it('sorts by stars by default (high first)', () => {
    render(<RepoList repos={mockRepos} />);
    const links = screen.getAllByRole('link', { name: /^(alpha|beta|gamma)$/ });
    expect(links[0]).toHaveTextContent('gamma');
    expect(links[1]).toHaveTextContent('alpha');
    expect(links[2]).toHaveTextContent('beta');
  });

  it('filters by search term', async () => {
    const user = userEvent.setup();
    render(<RepoList repos={mockRepos} />);
    const search = screen.getByRole('searchbox', { name: /search/i });
    await user.type(search, 'beta');
    expect(screen.getByText('beta')).toBeInTheDocument();
    expect(screen.queryByText('alpha')).not.toBeInTheDocument();
    expect(screen.queryByText('gamma')).not.toBeInTheDocument();
  });

  it('shows empty message when search has no matches', async () => {
    const user = userEvent.setup();
    render(<RepoList repos={mockRepos} />);
    const search = screen.getByRole('searchbox', { name: /search/i });
    await user.type(search, 'nonexistent');
    expect(screen.getByText(/no repositories match your search/i)).toBeInTheDocument();
  });

  it('sorts by name when sort changed', async () => {
    const user = userEvent.setup();
    render(<RepoList repos={mockRepos} />);
    const sort = screen.getByRole('combobox', { name: /sort repositories/i });
    await user.selectOptions(sort, 'name');
    const links = screen.getAllByRole('link', { name: /^(alpha|beta|gamma)$/ });
    expect(links[0]).toHaveTextContent('alpha');
    expect(links[1]).toHaveTextContent('beta');
    expect(links[2]).toHaveTextContent('gamma');
  });
});
