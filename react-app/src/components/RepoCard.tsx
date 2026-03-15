import type { RepositorySummary } from '../types/repo';

interface RepoCardProps {
  repo: RepositorySummary;
}

export function RepoCard({ repo }: RepoCardProps) {
  return (
    <article className="repo-card" aria-labelledby={`repo-name-${repo.Name}`}>
      <h3 id={`repo-name-${repo.Name}`} className="repo-card__name">
        <a
          href={repo.Url}
          target="_blank"
          rel="noopener noreferrer"
          className="repo-card__link"
        >
          {repo.Name}
        </a>
      </h3>
      {repo.Description ? (
        <p className="repo-card__description">{repo.Description}</p>
      ) : (
        <p className="repo-card__description repo-card__description--muted">
          No description
        </p>
      )}
      <dl className="repo-card__meta">
        <div className="repo-card__meta-item">
          <dt>Stars</dt>
          <dd>{repo.Stars}</dd>
        </div>
        {repo.PrimaryLanguage ? (
          <div className="repo-card__meta-item">
            <dt>Language</dt>
            <dd>{repo.PrimaryLanguage}</dd>
          </div>
        ) : null}
        <div className="repo-card__meta-item">
          <dt>Updated</dt>
          <dd>{repo.LastUpdated}</dd>
        </div>
      </dl>
      <a
        href={repo.Url}
        target="_blank"
        rel="noopener noreferrer"
        className="repo-card__cta"
      >
        View on GitHub
      </a>
    </article>
  );
}
