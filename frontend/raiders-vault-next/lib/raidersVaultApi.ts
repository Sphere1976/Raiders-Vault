export type LiveCondition = {
  map: string;
  condition: string;
  status: string;
  timeWindow: string;
  summary: string;
};

export type NewsItem = {
  title: string;
  publishedAt: string;
  source: string;
  url: string;
  summary: string;
};

export type GlobalOpsResponse = {
  generatedAtUtc: string;
  liveOps?: {
    sourceName: string;
    sourceUrl: string;
    activeConditions: LiveCondition[];
    news: NewsItem[];
  };
};

const fallback: GlobalOpsResponse = {
  generatedAtUtc: new Date().toISOString(),
  liveOps: {
    sourceName: 'Raiders Vault fallback snapshot',
    sourceUrl: 'http://127.0.0.1:5217/GlobalOps/Index',
    activeConditions: [
      {
        map: 'The Blue Gate',
        condition: 'Harvester',
        status: 'Fallback',
        timeWindow: 'Current rotation',
        summary: 'Use fallback data when the protected MVC API is unavailable in local frontend-only development.'
      }
    ],
    news: [
      {
        title: 'Connect the protected Raiders Vault API',
        publishedAt: new Date().toISOString().slice(0, 10),
        source: 'Raiders Vault',
        url: 'http://127.0.0.1:5217/GlobalOps/Index',
        summary: 'Run the ASP.NET Core app and provide an authenticated session to replace this fallback.'
      }
    ]
  }
};

export async function getGlobalOps(): Promise<GlobalOpsResponse> {
  const baseUrl = process.env.RAIDERS_VAULT_API_URL ?? 'http://127.0.0.1:5217';

  try {
    const response = await fetch(`${baseUrl}/api/v1/global-ops`, {
      cache: 'no-store',
      headers: {
        accept: 'application/json'
      }
    });

    if (!response.ok) {
      return fallback;
    }

    return (await response.json()) as GlobalOpsResponse;
  } catch {
    return fallback;
  }
}
