# 0006 · Delivery: build once, staging by merge, production by review, canary by measure

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: a bad version reaching every user, a rollback that does not happen, or a schema change that cannot be undone
- **Changed since the last version:** first version

Status: Superseded by 0012 (accepted 2026-11-09; superseded 2026-11-09, design amendment 1)
Decider: Prashant Sharma (tech lead, and release owner for E13 to E17) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3

## Context
NFR-REL-01 to 04, NFR-DATA-01, requirements 9.4 and the spec (AC20, AC21, AC23, AC25, E13 to E15, E17) decide what
a release must do. The challenge (Challenged C6 to C8) found that a weighted canary of a Next.js app without
stickiness bounces a browser between versions, that the promotion check read intent rather than facts, and that the
analysis means nothing at demo traffic.

## Drivers
- NFR-REL-02 (Decided): 20%, 50%, 100%, rolling back by itself above 5% errors or 500 ms p95.
- AC20: no request fails because of a release; sessions survive a rollout and a rollback.
- AC23: production moves only to a version staging runs, or one production ran before.
- AC25, NFR-DATA-01: expand and contract.

## Options
A. Argo Rollouts canaries with Traefik's own weighted service (`TraefikService`, with a sticky cookie) referenced from
   the Gateway API route; web-to-API calls through an internal Traefik listener, so the API canaries the same way;
   analysis on Traefik's per-service metrics with a minimum request count and generated load; a release ledger written
   by the cluster on each completed or aborted rollout — **chosen**, subject to U2 and U7.
B. As A, but blue/green for the web app (a preview, then one switch) and canary for the API only — rejected for now:
   it removes the skew problem, but changes a Decided requirement (NFR-REL-02) for the web app. It is the fallback if
   U2 fails, and needs the owner's decision.
C. A rolling update with readiness checks and a manual rollback — rejected: the cheapest, but it fails NFR-REL-02
   and AC21.

## Decision
- **Build once.** On `push` to `main` in `jobwright-app`, CI builds the web, API and worker images once, writes a bill
  of materials and scans each (fixable high or critical fails, AC24), then pushes them to the local registry and
  records their digests.
- **Staging by merge.** The same job commits the three digests to `environments/staging/values.yaml` in
  `jobwright-gitops` as `jobwright-bot`, pushing straight to `main`: that file is under the unprotected pattern
  `environments/staging/**`, the only change to `main` that needs no pull request (ADR 0005, U14). A push that touches
  any other file is refused. Argo CD syncs staging by itself (AC23).
- **Production by review.** A person, or a "propose promotion" workflow they start, opens a pull request that copies
  digests into `environments/production/values.yaml`. Its required check `checks/promotion` passes only when every
  digest is either the stable digest that completed a staging rollout and still runs there, or a digest that
  completed a production rollout before; it reads these facts from the release ledger.
- **Release ledger.** A branch `release-ledger` in `jobwright-gitops`, written only by the cluster: Argo Rollouts'
  notifications post each completed or aborted rollout (environment, workload, digest, result, time) through the
  Gitea API as one new file per event. Force pushes are blocked on it and on `main`. (U7: whether the notification
  can create the file directly is checked in slice 0; the fallback is a small in-cluster writer.)
- **Canary.** Each workload is an Argo Rollouts `Rollout`. The web app's public route and the API's internal route
  point at a `TraefikService` that weights the stable and canary services, with a sticky cookie so one browser stays
  on one version during the canary (Challenged C6). Steps: 20%, analysis, 50%, analysis, 100%. (U2: Traefik documents
  `TraefikService` backendRefs from an HTTPRoute, allowed by `crossProviderNamespaces`, set explicitly; Rollouts
  documents weighting a `TraefikService`; Traefik documents a sticky cookie on weighted services; that the three work
  together is INFERENCE, proved in slice 0; the fallback is option B, which needs the owner.)
- **Next.js skew.** `deploymentId` is the commit, and the server-actions encryption key is shared by all versions (a
  sealed secret), so a browser that meets a new version reloads once instead of failing.
- **Analysis** (Prometheus, Traefik's per-service metrics): error rate is 5xx over all requests for the canary's
  service; p95 from the request-duration histogram, with buckets added at 0.25, 0.5 and 0.75 seconds. A measurement
  counts only with at least 100 canary requests in its window; below that it is inconclusive, the step waits, and after
  three inconclusive measurements the release rolls back (E15). During each step a load job sends traffic to the canary
  It signs in as a person of a dedicated probe tenant, created by the tenant command in each environment
  (U8, decided by Prashant Sharma, release owner, 2026-11-08): the probe tenant holds no demo data, is left out of the
  demo screens, and its person's password is a sealed secret.
- **Rollback.** An aborted analysis returns all traffic to stable at once, records the abort in the ledger and leaves
  the Rollout degraded in Argo CD; the person then reverts the gitops commit so Git matches what runs. Sessions live in
  the database, not in pods, so they survive (AC20, E14). A version that never becomes ready takes no traffic and is
  aborted (E13).
- **Schema.** Migrations run as an Argo CD PreSync job (EF Core migration bundle, as `jobwright_owner`), are
  idempotent, and only expand in a release; contraction ships in a later release once no running version uses the old
  shape. Indexes are built concurrently in a migration without a transaction, with `lock_timeout` set. Every migration
  has a down path, run in CI. CI runs the previous release's integration tests against the new schema (AC25).
- **Platform outages.** Registry or Gitea unavailable: staging does not change and nothing running stops (E17).

## Consequences
- A production pull request to an unproven digest fails — enforced by `checks/promotion` (required check, AC23), shown
  red with fixture pull requests.
- A bad canary rolls back by itself — enforced by the staging release drill (AC21), with a deliberately failing
  version.
- A schema change that breaks the previous version fails CI — enforced by AC25's job.
- A migration without a tested down path fails CI — enforced by a job that applies every migration up, down and up.
- **What cannot be undone:** a contraction step that drops data. It ships alone, after the data is copied, with the
  owner's approval in its pull request.
- Revisit if U2 fails (then option B, with the owner) or traffic grows enough to judge the canary without generated load.
