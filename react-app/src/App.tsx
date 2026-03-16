import { useEffect, useState } from 'react';
import type { RepositorySummary } from './types/repo';
import { RepoList } from './components/RepoList';
import './App.css';

const REPOS_JSON_URL = import.meta.env.VITE_REPOS_JSON_URL ?? '/repos.json';
const FETCH_RETRY_ATTEMPTS = 3;
const FETCH_RETRY_DELAY_MS = 1000;

async function fetchWithRetry(
  url: string,
  attempts = FETCH_RETRY_ATTEMPTS
): Promise<Response> {
  let lastError: Error | null = null;
  for (let attempt = 1; attempt <= attempts; attempt++) {
    try {
      const res = await fetch(url);
      if (res.ok) return res;
      if (res.status >= 400 && res.status < 500) throw new Error(`Failed to load: ${res.status} ${res.statusText}`);
      lastError = new Error(`Failed to load: ${res.status} ${res.statusText}`);
    } catch (e) {
      lastError = e instanceof Error ? e : new Error('Failed to load');
    }
    if (attempt < attempts) {
      await new Promise((r) => setTimeout(r, FETCH_RETRY_DELAY_MS * attempt));
    }
  }
  throw lastError;
}

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
        const res = await fetchWithRetry(REPOS_JSON_URL);
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
