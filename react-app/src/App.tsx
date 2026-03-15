import { useEffect, useState } from 'react';
import type { RepositorySummary } from './types/repo';
import { RepoList } from './components/RepoList';
import './App.css';

const REPOS_JSON_URL = '/repos.json';

function App() {
  const [repos, setRepos] = useState<RepositorySummary[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadRepos() {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(REPOS_JSON_URL);
        if (!res.ok) {
          throw new Error(`Failed to load: ${res.status} ${res.statusText}`);
        }
        const data = await res.json();
        if (!cancelled && Array.isArray(data)) {
          setRepos(data);
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : 'Failed to load repositories');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadRepos();
    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) {
    return (
      <main className="app" aria-busy="true" aria-live="polite">
        <p className="app__message">Loading repositories…</p>
      </main>
    );
  }

  if (error) {
    return (
      <main className="app">
        <p className="app__message app__message--error" role="alert">
          {error}
        </p>
        <p className="app__hint">
          Run the console app first to generate <code>repos.json</code>, or ensure{' '}
          <code>public/repos.json</code> exists for local development.
        </p>
      </main>
    );
  }

  if (!repos || repos.length === 0) {
    return (
      <main className="app">
        <h1 className="app__title">Top repositories</h1>
        <p className="app__message">No repositories to display.</p>
      </main>
    );
  }

  return (
    <main className="app">
      <header className="app__header">
        <h1 className="app__title">Top repositories</h1>
        <p className="app__subtitle">
          Data from the C# console app. Search and sort below.
        </p>
      </header>
      <RepoList repos={repos} />
    </main>
  );
}

export default App;
