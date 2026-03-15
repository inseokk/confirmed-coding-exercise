/**
 * Repository summary — matches the JSON schema produced by the C# console app.
 */
export interface RepositorySummary {
  Name: string;
  Description: string;
  Stars: number;
  PrimaryLanguage: string;
  Url: string;
  LastUpdated: string;
}
