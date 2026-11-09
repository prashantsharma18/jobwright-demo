# 0009 · Telemetry: straight to the lab, with personal data kept out in code

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: personal data reaching logs, traces or metrics (E26)
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09)
Decider: Prashant Sharma (tech lead, and privacy reviewer for E26) · Agreed: Prashant Sharma (owner of the lab stack) · Work item: NEW-jobwright · Tier: 3

## Context
NFR-OPS-01 sends traces, metrics and logs from every part to the owner's lab collector through OpenTelemetry.
NFR-OPS-04 makes every part report its version. E26 keeps names, emails and client addresses out of telemetry from
this increment. E16: when the collector is down, the product keeps working.

## Drivers
- AC18, AC19, E16, E26.
- Memory: at most 6 GB for the cluster (E21).
- Port 4318 belongs to the lab; Jobwright sends to it and never binds it (requirements 9.5).

## Options
A. Each part exports OTLP over HTTP straight to the lab collector on the host, with batching and a bounded queue
   that drops when full; personal data is kept out by an allowlist of attributes in each part's telemetry setup —
   **chosen**.
B. An OpenTelemetry Collector in the cluster that receives from every part, removes personal attributes with a
   processor, buffers and forwards to the lab — rejected for this increment: a second line of defence, but another
   component to run (memory) and to keep; the allowlist in A plus the test is the control.

## Decision
- Every part (web, API, worker) uses the OpenTelemetry SDK of its runtime (pinned in `new-system.md`), exporting OTLP
  over HTTP to the lab collector through the host address that k3d gives the cluster. The address is configuration,
  never code.
- `service.name` is `jobwright-web`, `jobwright-api` or `jobwright-worker`; `service.version` is the commit the part
  was built from (AC18); `deployment.environment` is `staging` or `production`.
- Exports are batched with a bounded queue; when the collector does not answer, data is dropped after the queue fills,
  and requests are never slowed or failed by telemetry (E16).
- Personal data (E26): request and log attributes pass an allowlist; client addresses, names, emails, cookies and the
  `Authorization` header are never recorded; URL query strings are not recorded; log messages use templates with
  identifiers (tenant id, person id), never names or emails.
- AC19 is proved by a check in staging: after a seeded sign-in, page view and error, a search of the lab's logs and
  traces for the seeded persons' names and emails, and the test client's address, finds nothing.

## Consequences
- A part without its version in telemetry fails AC18's test — enforced by the integration test that reads the
  exported resource attributes.
- Personal data in telemetry fails AC19's staging check — enforced by that check, run in every release to staging.
- An attribute added outside the allowlist is dropped, not exported — enforced by the allowlist, with a unit test per
  part that records a request carrying every forbidden value and asserts none is exported.
- Revisit if telemetry must leave the machine, or a second consumer of telemetry appears: then add option B.
