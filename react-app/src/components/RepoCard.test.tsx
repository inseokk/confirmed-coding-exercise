import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { RepoCard } from './RepoCard';
import type { RepositorySummary } from '../types/repo';

const mockRepo: RepositorySummary = {
  Name: 'test-repo',
  Description: 'A test repository',
  Stars: 42,
  PrimaryLanguage: 'TypeScript',
  Url: 'https://github.com/user/test-repo',
  LastUpdated: '2025-01-15 12:00 UTC',
};

describe('RepoCard', () => {
  it('renders repo name as link', () => {
    render(<RepoCard repo={mockRepo} />);
    const link = screen.getByRole('link', { name: 'test-repo' });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', mockRepo.Url);
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('renders description', () => {
    render(<RepoCard repo={mockRepo} />);
    expect(screen.getByText('A test repository')).toBeInTheDocument();
  });

  it('renders "No description" when description is empty', () => {
    render(<RepoCard repo={{ ...mockRepo, Description: '' }} />);
    expect(screen.getByText('No description')).toBeInTheDocument();
  });

  it('renders stars, language, and last updated', () => {
    render(<RepoCard repo={mockRepo} />);
    expect(screen.getByText('42')).toBeInTheDocument();
    expect(screen.getByText('TypeScript')).toBeInTheDocument();
    expect(screen.getByText('2025-01-15 12:00 UTC')).toBeInTheDocument();
  });

  it('renders View on GitHub link', () => {
    render(<RepoCard repo={mockRepo} />);
    const cta = screen.getByRole('link', { name: 'View on GitHub' });
    expect(cta).toHaveAttribute('href', mockRepo.Url);
  });
});
