import { useMemo, useState } from 'react';
import type { RepositorySummary } from '../types/repo';
import { RepoCard } from './RepoCard';

type SortKey = 'stars' | 'name' | 'updated';

interface RepoListProps {
  repos: RepositorySummary[];
}

export function RepoList({ repos }: RepoListProps) {
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState<SortKey>('stars');

  const filteredAndSorted = useMemo(() => {
    const trimmed = search.trim().toLowerCase();
    let list = repos;

    if (trimmed) {
      list = list.filter(
        (r) =>
          r.Name.toLowerCase().includes(trimmed) ||
          (r.Description && r.Description.toLowerCase().includes(trimmed))
      );
    }

    return [...list].sort((a, b) => {
      switch (sortBy) {
        case 'stars':
          return b.Stars - a.Stars;
        case 'name':
          return a.Name.localeCompare(b.Name, undefined, { sensitivity: 'base' });
        case 'updated':
          return new Date(b.LastUpdated).getTime() - new Date(a.LastUpdated).getTime();
        default:
          return 0;
      }
    });
  }, [repos, search, sortBy]);

  return (
    <div className="repo-list">
      <div className="repo-list__controls">
        <label htmlFor="repo-search" className="repo-list__label">
          Search
        </label>
        <input
          id="repo-search"
          type="search"
          placeholder="Filter by name or description..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="repo-list__search"
          aria-describedby="repo-search-hint"
        />
        <span id="repo-search-hint" className="repo-list__hint">
          {filteredAndSorted.length} of {repos.length} repo{repos.length !== 1 ? 's' : ''}
        </span>

        <label htmlFor="repo-sort" className="repo-list__label">
          Sort by
        </label>
        <select
          id="repo-sort"
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value as SortKey)}
          className="repo-list__sort"
          aria-label="Sort repositories"
        >
          <option value="stars">Stars (high to low)</option>
          <option value="name">Name (A–Z)</option>
          <option value="updated">Last updated</option>
        </select>
      </div>

      {filteredAndSorted.length === 0 ? (
        <p className="repo-list__empty">
          {search.trim() ? 'No repositories match your search.' : 'No repositories to show.'}
        </p>
      ) : (
        <ul className="repo-list__grid" role="list">
          {filteredAndSorted.map((repo) => (
            <li key={repo.Url}>
              <RepoCard repo={repo} />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
