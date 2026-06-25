import Link from 'next/link';

const evidence = [
  ['React + TypeScript', 'Componentized Next.js console with strict types and server-side data loading.'],
  ['Spring Boot', 'Companion Java API service for live operations and downstream integrations.'],
  ['AWS + Terraform', 'Infrastructure blueprint for ECS Fargate, ALB, CloudWatch, and deployable containers.'],
  ['Testing', 'Playwright, JUnit, and Postman assets aligned to common full-stack quality gates.']
];

export default function HomePage() {
  return (
    <>
      <section className="hero">
        <div>
          <span className="eyebrow">Full-stack modernization track</span>
          <h1>Raiders Vault Engineering Console</h1>
          <p>
            A Next.js companion experience for the existing Raiders Vault platform, built to demonstrate
            React, TypeScript, REST integration, cloud readiness, and test automation.
          </p>
          <div className="button-row">
            <Link className="button" href="/global-ops">Open Global Ops</Link>
          </div>
        </div>
        <div className="panel">
          <span className="label">Hiring alignment</span>
          <h2>Production evidence</h2>
          <p>
            This frontend is intentionally separated from the ASP.NET Core MVC app so reviewers can see
            modern SPA/SSR architecture and typed API boundaries without replacing the working capstone.
          </p>
        </div>
      </section>

      <section className="grid">
        {evidence.map(([title, copy]) => (
          <article className="card" key={title}>
            <span className="label">{title}</span>
            <p>{copy}</p>
          </article>
        ))}
      </section>
    </>
  );
}
