# Confirmed Software Developer Internship - Coding Exercise

## What I built

**Part A - C#/.NET console application**

- Accepts a GitHub username as a command-line argument.
- Uses the GitHub public API (GET /users/{username}/repos) to fetch that user's public repositories.
- Maps the API response into an internal C# model (RepositorySummary).
- Fetches all pages (pagination, per_page=100) so the top 5 by stargazer count are correct for users with many repos.
- Prints readable console output: repository name, description, stars, primary language, repository URL, last updated date.
- Writes the normalized results to a local JSON file (repos.json by default) for the React app to consume.
- Uses HttpClient, deserializes JSON into C# models, and handles invalid input, empty results, 404, and failed HTTP responses (with retry for transient/5xx).

**Part B - React interface**

- Small React (Vite + TypeScript) app that displays the normalized repo data.
- Loads data from the JSON file produced by the console app (or from public/repos.json / sample file for local development).
- Displays records in a clean, readable UI (cards with name, description, stars, language, URL, last updated).
- Interactive features: **search** (filter by name or description) and **sort** (by stars, name A-Z, or last updated).

**Optional enhancements included**

- **Unit tests** - C#: xUnit in console-app.Tests/. React: Vitest + Testing Library in react-app/src.
- **Dependency injection** - C#: ServiceCollection, AddHttpClient, config and logging from the container.
- **Configuration** - C#: appsettings.json plus environment variables (e.g. Output__FilePath, GitHub__ApiBaseUrl). React: VITE_REPOS_JSON_URL.
- **Logging** - C#: ILogger with console logging.
- **Retry** - C#: Polly (3 attempts, exponential backoff) for transient/5xx. React: fetchWithRetry for failed/5xx responses.
- **Pagination** - C#: GitHub API pagination so top 5 by stars is correct across all repos.

---

## How to run both parts

**Prerequisites:** .NET 10 SDK, Node.js (for the React app).

**Part A - Console app**

```bash
cd console-app
dotnet run -- <github-username>
```

Example: dotnet run -- inseokk

- Output is printed to the console and written to repos.json in the current directory (or the path in appsettings.json / env Output__FilePath).
- To run the console app tests: dotnet test console-app.Tests/console-app.Tests.csproj (from repo root or from console-app).

**Part B - React app**

1. For local development without running the console app, ensure react-app/public/repos.json exists (you can copy from console-app/repos.json after running Part A once).
2. From the repo root:

```bash
cd react-app
npm install
npm run dev
```

- Open the URL shown (e.g. http://localhost:5173). The app loads /repos.json by default; override with VITE_REPOS_JSON_URL if needed.
- To run the React tests: npm run test.

---

## Assumptions

- The console app is run from the console-app directory (or with a working directory where appsettings.json is present) so config and output paths resolve correctly.
- The React app is served by Vite dev server or similar, and repos.json is either in public/ or provided via the same origin (or VITE_REPOS_JSON_URL).
- JSON schema: same as the C# RepositorySummary (Name, Description, Stars, PrimaryLanguage, Url, LastUpdated). The React app expects this schema.
- GitHub API is used unauthenticated; rate limits apply for heavy use.


## What I would improve with more time

- **React:** Add filter by primary language and/or expand/collapse per card; improve loading/error UX (e.g. skeleton, retry button).
- **C#:** Add more unit tests (e.g. pagination behavior, retry behavior); optional integration test against a test GitHub user or mocked server.
- **DevOps:** Add a simple script or instructions to run the console app and copy repos.json into react-app/public/ for a one-command full demo flow.
- **Accessibility:** Audit and improve keyboard navigation and screen reader support in the React app.