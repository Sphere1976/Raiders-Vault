# LiveOps Resilience Drill

## Scenario

The upstream live-ops feed is unavailable or slow while users are opening Global Ops.

## Expected Behavior

- MVC app remains healthy.
- Global Ops page renders fallback live-ops data.
- Next.js console renders fallback data if the protected MVC API is unavailable.
- Spring LiveOps service continues returning deterministic cached data.
- Logs include enough context to identify upstream failure mode.

## Manual Drill

1. Start the MVC application.
2. Block or disconnect external network access.
3. Open `/GlobalOps/Index` with an authenticated session.
4. Confirm the live map condition banner still renders.
5. Start the Next.js console with no authenticated MVC session.
6. Confirm `/global-ops` still renders fallback Global Ops cards.

## Future Automation

- Add Toxiproxy between services for latency and failure injection.
- Add k6 scenarios that simulate degraded upstream latency.
- Emit a `LiveOpsFallbackActivated` event to EventBridge.
- Alert if fallback mode remains active longer than 30 minutes.
