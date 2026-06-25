import type { GlobalOpsResponse } from '../lib/raidersVaultApi';

type Props = {
  data: GlobalOpsResponse;
};

export function GlobalOpsDashboard({ data }: Props) {
  const liveOps = data.liveOps;
  const activeConditions = liveOps?.activeConditions ?? [];
  const news = liveOps?.news ?? [];

  return (
    <>
      <section className="hero">
        <div>
          <span className="eyebrow">Live operations</span>
          <h1>Global Ops</h1>
          <p>
            Server-rendered React view over Raiders Vault operational intelligence. It is designed for
            quick status checks, live map condition review, and enterprise dashboard embedding.
          </p>
        </div>
        <div className="panel">
          <span className="label">Source</span>
          <h2>{liveOps?.sourceName ?? 'Unavailable'}</h2>
          <p>Generated {new Date(data.generatedAtUtc).toLocaleString()}</p>
          {liveOps?.sourceUrl ? <a className="button" href={liveOps.sourceUrl}>Open source</a> : null}
        </div>
      </section>

      <section className="grid">
        <article className="card">
          <span className="label">Active conditions</span>
          <strong>{activeConditions.length}</strong>
        </article>
        <article className="card">
          <span className="label">News updates</span>
          <strong>{news.length}</strong>
        </article>
        <article className="card">
          <span className="label">REST boundary</span>
          <strong>v1</strong>
        </article>
      </section>

      <section className="grid">
        <article className="panel">
          <span className="label">Map conditions</span>
          <h2>Live Banner Feed</h2>
          <div className="condition-list">
            {activeConditions.map(condition => (
              <div className="condition-row" key={`${condition.map}-${condition.condition}`}>
                <span className="status">{condition.status}</span>
                <strong>{condition.condition}</strong>
                <small>{condition.map}</small>
                <small>{condition.timeWindow}</small>
                <p>{condition.summary}</p>
              </div>
            ))}
          </div>
        </article>

        <article className="panel">
          <span className="label">Embark updates</span>
          <h2>News Feed</h2>
          <div className="news-list">
            {news.map(item => (
              <a className="news-card" href={item.url} key={item.title}>
                <strong>{item.title}</strong>
                <small>{item.source} / {item.publishedAt}</small>
                <p>{item.summary}</p>
              </a>
            ))}
          </div>
        </article>
      </section>
    </>
  );
}
