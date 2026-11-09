<!-- aidlc template: design-note -->
# NEW-jobwright · design note

## For the reviewer
- **Asked of:** The tech lead or architect, to approve the design or record "none needed"; consumers approve a contract change; Tier 3 adds the specialists
- **Record it:** On the APPROVED or RECORDED BY line below, and on the work item with this note's version and commit
- **Tier and main risk:** 3: one company reaching another's data, a weakness in signing in, or a release that does not roll back
- **Changed since the last version:** first version

Inputs: [spec v1](spec.md) (approved by Prashant Sharma, 2026-11-08), [intent v2](intent.md), the
[requirements](../../docs/requirements/2026-11-08-jobwright-requirements.md). Route G: there is no code, map, rules
file or ADR; this design creates the architecture contract. With it: [new-system.md](new-system.md),
[coordination-plan.md](coordination-plan.md), `adr-draft-1.md` to `adr-draft-10.md`, [contract/](contract/) and
[wireframes/](wireframes/).

```text
1 Does it change the system's shape (new service, store, dependency, integration pattern)?  yes — a new system: every part, store and dependency is new
2 Does it move a boundary (data ownership, security pattern, shared-module behaviour, new module dependency)?  yes — it sets every boundary: tenants, modules, sign-in, the gateway, CI and delivery
3 Does it change a contract someone outside the team relies on?  no — every contract is new and both ends belong to Prashant Sharma; the lab's collector is the owner's own and is only sent to

Uncertain counts as yes.

OUTCOME  Decisions D1 to D10, each owned by Prashant Sharma, each with a frame below and an ADR draft:
         D1 repositories and modules (adr-draft-1) · D2 sign-in, sessions, current tenant (adr-draft-2)
         D3 tenant isolation in the database (adr-draft-3) · D4 the API contract (adr-draft-4)
         D5 where CI runs before GitHub (adr-draft-5) · D6 delivery and canary (adr-draft-6)
         D7 platform and memory (adr-draft-7) · D8 gateway, client address, rate limit (adr-draft-8)
         D9 telemetry (adr-draft-9) · D10 the web shell (adr-draft-10)
UI REFERENCE  wireframes/app-shell.md v1 and wireframes/access-pages.md v1, approved by Prashant Sharma, 2026-11-08
              (no Figma file exists for this project; the wireframes are the reference)
RECORDED BY  Prashant Sharma, 2026-11-08
APPROVED  ADRs 0001 to 0010 now Accepted, approved by Prashant Sharma (tech lead; security, privacy and release reviewer under known limit I3), 2026-11-09 · contract Jobwright API v1 (contract/openapi.yaml) and its tests, approved by Prashant Sharma as its consumer, 2026-11-09 · boundary rules: as in the accepted ADRs
```

This session runs at the workspace root, which is read-only for code: no check below has run. Each is drafted now and
shown red in the first step of the slice that builds it; the plan names that step.

## Decision frames

Impact labels: FACT (with its source), INFERENCE (reasoning), UNKNOWN (with an owner). There is no code, so FACTs
cite the requirements, the spec or a vendor's documentation.

### D1 · Repositories, parts and module boundaries
```text
Decision  How are the parts and modules laid out, and how are module boundaries kept?   Owner: Prashant Sharma
Drivers   Requirements 9.1, 9.2; FR-IAM-02; AC22
Options   A one project per module + Contracts projects + architecture test   B folders + convention (cheapest)   C one repository
Out       Splitting the API into services
Unknown   none
```
```text
FACT       Two repositories and one API with modules are Decided (requirements 9.1, 9.2)
INFERENCE  Without a compiler or test boundary, a module will read another's tables within a few increments
A          Changes: the solution layout. Drivers: boundary enforced in CI. Reverse: merge projects (cheap)
B          Changes: none. Drivers: boundary by review only. Reverse: n/a
RECOMMEND  A
AGAINST    More projects and references to keep for a small team of one; B is faster today
```

### D2 · Sign-in, sessions and the current tenant
```text
Decision  How does a person sign in, keep a session, and act in one tenant?   Owner: Prashant Sharma (risk owner)
Drivers   NFR-SEC-01; AC1 to AC3, AC6, AC14; E1 to E8, E12; FR-IAM-02
Options   A server-side session holds the tenant; token proves the person; API checks membership per request
          B token scoped to one organization   C tokens in an encrypted cookie (cheapest)
Out       Federation with each company's identity provider (S4: not in version 1)
Unknown   U5 (where the CSP nonce is set) only touches D10
```
```text
FACT       Keycloak 26.8 offers the scopes `organization`, `organization:<alias>` and `organization:*`, not mixable in
           one request; the claim is `{"<alias>": {...}}` (keycloak.org/docs/latest/server_admin, organizations)
FACT       Reported: a token for organization B carried A, pinned at login (issues.redhat.com/browse/RHBK-2642), and the
           claim included inconsistently (RHBK-2578)
FACT       Keycloak's SSO Session Idle defaults to 30 minutes, below the spec's 60 (Challenged C5; realm settings fix it)
INFERENCE  A per-request membership check costs one indexed lookup; acceptable within NFR-PERF-01's 300 ms
A          Changes: web_sessions table, BFF routes, API header check, network policy. Drivers: AC2 and E2 exact.
           Reverse: replace the header by a claim later (option B) without changing the browser
B          Changes: none in the database. Drivers: switching tenant needs a sign-in round trip; claim unreliable
C          Changes: none. Drivers: fails AC2 (no server-side end)
RECOMMEND  A
AGAINST    The tenant travels in a header the API must trust; its safety rests on network policy plus the
           membership check, not on a signed claim
```

### D3 · Tenant isolation in the database
```text
Decision  How does the database keep tenants apart even when code forgets?   Owner: Prashant Sharma (risk owner)
Drivers   AC4, AC5, E5, E20; requirements 9.1 (isolation by the database)
Options   A forced row-level security + composite keys + separate roles   B schema per tenant   C application filter (cheapest)
Out       Cross-tenant reporting (none in version 1)
Unknown   none
```
```text
FACT       Constraints (unique, foreign key) are checked without row-level security (PostgreSQL behaviour; Challenged C2)
INFERENCE  A transaction-local setting leaves an empty value on a pooled session; a direct uuid cast would then fail
A          Changes: every tenant-owned table, four roles, an interceptor. Drivers: AC5 holds below the code.
           Reverse: policies can be dropped, but nothing should depend on that
B          Changes: a schema per tenant. Drivers: migration per tenant; search-path mistakes cross tenants
C          Changes: a global filter. Drivers: fails AC5 by design
RECOMMEND  A
AGAINST    Row-level security adds a per-query cost and a class of silent "no rows" bugs; the interceptor that fails
           loudly is the mitigation, and it is code that must itself be right
```

### D4 · The API contract
```text
Decision  How is the API's contract kept as the source, and how are errors shaped?   Owner: Prashant Sharma
Drivers   Section 8 (contract tests); NFR-SEC-02, 03; AC4, AC13
Options   A hand-written OpenAPI + drift test + breaking-change check + generated client   B code first (cheapest)   C broker-based consumer contracts
Out       A public API for third parties
Unknown   whether the framework's generated document can match the hand-written file exactly -> Prashant Sharma, first slice of part 3
```
```text
FACT       In .NET 10 the built-in generator emits OpenAPI 3.1 by default (learn.microsoft.com, aspnetcore-openapi, aspnetcore-10.0)
INFERENCE  It may still differ in form (ordering, nullable style); the test normalises before comparing
A          Changes: contract file, two tests, one tool. Drivers: drift fails CI on both sides. Reverse: drop the test
B          Changes: none. Drivers: the contract follows the code
C          Changes: a broker. Drivers: one consumer in the same repository; cost without benefit
RECOMMEND  A
AGAINST    The hand-written contract and the generator may fight over details; that friction is the price of
           contract first
```

### D5 · Where CI runs before GitHub
```text
Decision  What runs the workflows and enforces checks and reviews while nothing may be pushed?   Owner: Prashant Sharma
Drivers   Section 8; requirements 9.2, 9.4, 9.5; AC24, AC27
Options   A Gitea + runner with an isolated Docker-in-Docker daemon   B local runs only (cheapest)   C GitHub now
          D Forgejo + runner
Out       Hosted runners
Unknown   U6 how pull requests are authored and approved with one person -> Prashant Sharma
```
```text
FACT       Forgejo Actions ignores `permissions` and `continue-on-error`; `uses:` resolves against DEFAULT_ACTIONS_URL
           (forgejo.org/docs/latest/user/actions/github-actions/, forgejo.org/docs/next/admin/actions/)
FACT       Forgejo cannot require a code owner's approval (Forgejo issue #5179, open); Gitea 28 has "Require approval
           from code owners" (docs.gitea.com/usage/code-owners; Gitea 28.3.0)
INFERENCE  A runner holding Docker Desktop's socket could exec into the k3d node and act as cluster admin (Challenged C9)
A          Changes: Gitea, runner, dind on the machine. Drivers: checks enforced. Reverse: move to GitHub (option C)
B          Changes: none. Drivers: no enforcement
C          Changes: remotes. Drivers: forbidden by 9.2 until the owner asks
RECOMMEND  A
AGAINST    A privileged Docker-in-Docker daemon in the cluster's VM is still a path to the cluster (accepted risk R1)
```

### D6 · Delivery and canary
```text
Decision  How does a version reach staging and production, roll out in steps and roll back?   Owner: Prashant Sharma (release owner)
Drivers   NFR-REL-01 to 04, NFR-DATA-01; AC20, AC21, AC23, AC25; E13 to E17
Options   A Rollouts + TraefikService weights with stickiness + ledger + measured analysis   B blue/green for the web, canary for the API
          C rolling update (cheapest)
Out       The production canary drill (increment 4, S11)
Unknown   U2 route to a TraefikService and Rollouts' control of it -> checked against docs and in slice 0, Prashant Sharma
          U7 ledger written by Rollouts notifications -> slice 0, Prashant Sharma
          U8 what the canary load job signs in as -> Prashant Sharma (release owner)
```
```text
FACT       Next.js documents version-skew failures and the fix (deploymentId, shared server-actions key)
           (nextjs.org/docs/app/guides/self-hosting#version-skew)
FACT       Traefik's default duration buckets are 0.1, 0.3, 1.2 and 5 s (doc.traefik.io, observability metrics)
INFERENCE  At demo traffic a canary sees few requests; a ratio over 3 requests is noise (Challenged C8)
A          Changes: Rollouts, TraefikService, ledger, load job, analysis templates. Drivers: NFR-REL-02 as Decided.
           Reverse: abort returns traffic to stable at once
B          Changes: as A, minus web weights. Drivers: no skew for the web; deviates from NFR-REL-02 for the web
C          Changes: none. Drivers: fails NFR-REL-02
RECOMMEND  A
AGAINST    The most moving parts of the design (two UNKNOWNs) for a demo-scale system; B is simpler and may be what
           the owner prefers
```

### D7 · Platform and memory
```text
Decision  What runs in increment 1, where, and does it fit 6 GB?   Owner: Prashant Sharma (owner of the machine)
Drivers   E21, AC28; requirements 9.4, 9.5; spec A4
Options   A one cluster, namespaces per environment, only what increment 1 uses   B a cluster per environment   C everything now
Out       Object storage, flags, the live-update backplane (deferred)
Unknown   U9 the measured idle total -> slice 0 of jobwright-gitops, Prashant Sharma
```
```text
FACT       Keycloak's sizing guide puts a pod with realm caches near 1,250 MB (keycloak.org, memory and CPU sizing)
INFERENCE  The increment-1 platform totals about 5.75 GB (adr-draft-7); unmeasured
A          Changes: one cluster. Drivers: fits, barely. Reverse: split later if memory allows
B          Changes: two clusters. Drivers: over budget
C          Changes: more components. Drivers: over budget, unused
RECOMMEND  A
AGAINST    Shared platform for both environments (accepted risk R3), and almost no headroom
```

### D8 · Gateway, client address and rate limit
```text
Decision  How does traffic enter, and how are sign-in attempts limited per client?   Owner: Prashant Sharma (risk owner)
Drivers   NFR-SEC-04, 05; AC15, AC16; E9, E18; NFR-REL-04
Options   A Traefik chart, peer address, no trusted forwarded headers, measured   B trusted front proxy header   C limit in the web app (cheapest)
Out       A public tunnel (needs B by amendment)
Unknown   U1 what address Traefik sees -> slice 0, and the risk owner's acceptance of "this machine is one client"
          U3 middleware on an HTTPRoute through ExtensionRef -> official docs, Prashant Sharma
```
```text
FACT       k3d's load balancer is an nginx stream proxy with no PROXY protocol, so Traefik sees the load balancer's
           address (github.com/k3d-io/k3d/blob/v5.9.0/proxy/templates/nginx.tmpl)
FACT       Traefik's Gateway API provider supports middlewares as ExtensionRef filters when its Kubernetes CRD provider
           is on (doc.traefik.io, Gateway API routing reference); shown there with AddPrefix, so RateLimit is INFERENCE
A          Changes: Traefik config and middleware. Drivers: E18 exact; per-client only if U1 allows. Reverse: config
B          Changes: a front proxy. Drivers: correct per-client behind one proxy; none exists
C          Changes: web code. Drivers: Keycloak's form unprotected
RECOMMEND  A
AGAINST    On this machine the "per client" limit may in practice be one limit for everyone; the requirement's words
           are then met only because every real client is this machine
```

### D9 · Telemetry
```text
Decision  How does telemetry reach the lab without personal data?   Owner: Prashant Sharma (privacy reviewer)
Drivers   NFR-OPS-01, 04; AC18, AC19; E16, E26
Options   A direct OTLP with an attribute allowlist   B an in-cluster collector with a redaction processor
Out       Dashboards and alerts (increment 4)
Unknown   none
```
```text
INFERENCE  An in-cluster collector costs roughly 100 MB; the allowlist plus AC19's check is the control in A
A          Changes: SDK setup per part. Drivers: met. Reverse: add B later without code changes (endpoint config)
B          Changes: a collector. Drivers: second line of defence; memory
RECOMMEND  A
AGAINST    One missed attribute in one part reaches the lab with no second filter
```

### D10 · The web shell
```text
Decision  How are pages rendered, themed, secured and kept within budget?   Owner: Prashant Sharma
Drivers   AC7 to AC12, AC14, AC15, AC26; E10, E11, E19, E25; requirements 9.3
Options   A server rendering, cookie theme, build-time font, nonce CSP, browser-measured budget   B theme library, font CDN, static CSP, bundle analyzer
Out       The technician app (increment 3)
Unknown   U5 where Next.js 16 sets the per-request nonce -> official docs at slice 0, Prashant Sharma
          U4 what Settings shows in increment 1 -> Prashant Sharma (product owner)
```
```text
INFERENCE  A cookie the server reads avoids a flash of the wrong theme; local storage cannot
A          Changes: shell code. Drivers: met with no new dependency. Reverse: cheap
B          Changes: three dependencies. Drivers: flash, third-party font request, budget of files not bytes sent
RECOMMEND  A
AGAINST    A per-request nonce makes pages dynamic, which costs server rendering time on every request
```

## Threat model (securing-code, STRIDE over each trust boundary)

Assets: each tenant's data; people's names and emails; sessions and tokens; the ability to create tenants; the ability
to change what runs (staging, production); the sealed-secrets key.

| Boundary | Threats | Mitigations (where decided) |
|---|---|---|
| Browser -> gateway | S: stolen cookie; T: forged forwarded headers; D: sign-in flooding; I: framing, sniffing | `__Host-` cookie, httpOnly, Secure, SameSite=Lax (D2); no trusted forwarded headers (D8); rate limit (D8); headers and CSP (D10) |
| Gateway -> web server | E: a return address to another site; I: errors with internals | E4 path check (D2); problem responses only (D4) |
| Web server -> Keycloak | S: code interception; R: sign-out not ending Keycloak's session | PKCE S256, state, nonce; end-session with id_token_hint (D2) |
| Web server -> API | S: a forged tenant header; E: a role from the client | API reachable only from web pods (D2, D8); membership and role from the API's own records per request (D2) |
| API -> database | I, E: cross-tenant reads, joins or constraint leaks | Forced row-level security, composite keys, roles without BYPASSRLS, fail-loud interceptor (D3) |
| Worker -> database | I: a background job crossing tenants | One tenant per transaction, no cross-tenant query (D3) |
| Owner -> tenant command | E: creating a tenant on the wrong cluster | Local-cluster guard (AC22, E24); run only by the owner (S4) |
| CI -> registry, gitops | T, E: a pre-merge job changing staging or the cluster | Isolated runner, secrets only on `main` (D5, risks R1, R2); production by review and ledger (D6) |
| Cluster -> release ledger | T: a forged ledger entry | Only the cluster's bot token writes it; force push blocked (D6) |
| Parts -> lab telemetry | I: personal data in telemetry | Attribute allowlist and AC19's check (D9) |

## Challenged (stress-testing-decisions)

A fresh reviewer was given only the decision paragraphs and their assumptions, briefed to argue against them. Every
objection and its answer:

| ID | Objection | Severity | Answer |
|---|---|---|---|
| C1 | An empty tenant setting on a pooled connection makes the uuid cast fail instead of returning no rows | serious | Changed: `nullif(..., '')::uuid`; a test on a reused connection (ADR 0003, RowLevelSecurityTests) |
| C2 | Unique and foreign-key checks ignore row-level security, leaking existence and allowing cross-tenant references | serious | Changed: every key and constraint includes `tenant_id`; a schema check enforces it (ADR 0003) |
| C3 | Forced security binds migrations (silent zero-row backfills); views and definer functions bypass it; the worker has no path | serious | Changed: owner role with BYPASSRLS for migrations only; `security_invoker` views; no definer functions; worker per tenant (ADR 0003) |
| C4 | The setting is applied only inside a transaction; work outside it silently sees nothing; one transaction per request exhausts pools | serious | Changed: set at `TransactionStarted`; interceptor fails loudly; short units of work; pool sizes below `max_connections` (ADR 0003) |
| C5 | Single-organization tokens are unproven in Keycloak 26; realm idle default is 30 minutes; refresh races; sign-out may leave Keycloak's session | serious | Changed: current tenant held in the server session and checked by the API (option A); realm settings; locked refresh; front-channel end-session (ADR 0002) |
| C6 | A weighted Next.js canary without stickiness bounces browsers between versions and breaks navigation | blocker | Changed: sticky weighted service, `deploymentId`, shared server-actions key (ADR 0006); U2 checks the mechanism; fallback B needs the owner |
| C7 | The promotion check reads what was committed, not what ran; force push defeats it | serious | Changed: a ledger written by the cluster on each completed or aborted rollout; an allow-rule; force push blocked (ADR 0006); U7 |
| C8 | At demo traffic the analysis is noise or vacuous; Traefik's buckets cannot judge 500 ms; the plugin is unverified | serious | Changed: minimum 100 requests, load job, added buckets, inconclusive leads to rollback (E15); plugin replaced by TraefikService (ADR 0006); U8 |
| C9 | A runner with Docker Desktop's socket can take over the cluster | blocker | Changed: dedicated Docker-in-Docker daemon on an isolated network, no socket or kubeconfig (ADR 0005); residual accepted as R1 |
| C10 | Forgejo Actions differs from GitHub Actions (`permissions` ignored, actions URL); one person cannot approve their own pull request | serious | Changed: Gitea instead of Forgejo, because only Gitea can require a code owner's approval (version check); actions URL set to GitHub and `permissions` support checked (U12); token scope as the limit, R2 accepted (ADR 0005); approval question is U6 |
| C11 | The client address is likely lost behind Docker Desktop and k3d, so per-client limits become global; BFF calls to Keycloak through the gateway would share the limit | blocker | Changed: measured in slice 0 with a stated rule (U1, needs the risk owner); BFF uses Keycloak's back-channel inside the cluster (ADR 0002, ADR 0008) |
| C12 | About 5.6 to 5.9 GB with no headroom; CI builds in the same VM; shared Keycloak and platform for both environments; the sealed-secrets key can be lost | serious | Accepted with mitigations: measured in slice 0 with a stop rule; Keycloak's own database, never production's; risks R3, R4; key exported at setup (ADR 0007) |
| C13 | Minor: audience mapper; Node runtime only; permanent brute-force lockout; rate-limited paths; PreSync re-runs; concurrent index builds; bot commit races; `persons` without security | minor | Changed in ADR 0002 (audience, runtime), ADR 0008 (temporary lockout, paths, no direct grants), ADR 0006 (idempotent migrations, concurrent indexes, rebase and retry), ADR 0003 (column grants and a `security_invoker` view for `persons`) |

Risks accepted by Prashant Sharma, risk owner, 2026-11-08:
- R1 (ADR 0005), for the lab only: never for a shared or production environment.
- R2 (ADR 0005), on condition that only Prashant Sharma's and the `aidlc-agent` accounts push, and CI's secrets are
  scoped to the least they need (registry push only, no cluster access). The bot accounts are the decided exception,
  each with the narrowest right (U14).
- R3 (ADR 0007), for the demo; a known limit.
- R4 (ADR 0007): slice 0 measures memory with CI running and stops above 6 GB.
- U1's rule (ADR 0008), as a known limit: before any public tunnel or shared access, the rate-limit design changes.

Accepted by Prashant Sharma, risk owner, 2026-11-09: R5 (ADR 0005): an account with write access to `jobwright-gitops` can push a
staging-only change to `main` without review. Found while checking U14: Gitea's unprotected-file exception applies to
every account with write access, not only `jobwright-bot`.

## Unknowns

| ID | Question | Owner | How it is answered | Blocks |
|---|---|---|---|---|
| U1 | What source address Traefik sees from host clients and from pods. "This machine is one client" accepted by the risk owner, 2026-11-08, as a known limit (ADR 0008) | Prashant Sharma | Measured in slice 0 of `jobwright-gitops` | AC16 |
| U2 | Do an HTTPRoute to a `TraefikService`, Rollouts' weights on it and its sticky cookie work together? Each is documented on its own (doc.traefik.io, argo-rollouts.readthedocs.io); together is INFERENCE | Prashant Sharma | Slice 0 of `jobwright-gitops` | AC20, AC21; fallback option B needs the owner |
| U3 | Can a Traefik middleware attach to an HTTPRoute through an `ExtensionRef` filter? | Prashant Sharma | Official documentation | AC16 |
| U4 | Decided by Prashant Sharma, 2026-11-08: the tenant's name and time zone, read only (wireframes/app-shell.md) | — | — | — |
| U5 | Answered (FACT, nextjs.org CSP guide): in `proxy.ts`; pages render dynamically; Cache Components and partial prerendering off (ADR 0010) | — | — | — |
| U6 | Decided by Prashant Sharma, 2026-11-08, as proposed (ADR 0005). With one person, who opens pull requests and who approves them in Gitea? Gitea waives a code-owner rule whose only owner is the author (FACT, v28.3.0). Proposed: an `aidlc-agent` account opens every pull request (the person pushes with it); Prashant Sharma is the code owner and approves; a CI check refuses a pull request authored by the only code owner | Prashant Sharma | Decision at design approval | Every merge |
| U7 | Can Argo Rollouts' notifications write a ledger file through the Gitea API directly? | Prashant Sharma | Slice 0 of `jobwright-gitops`; fallback a small in-cluster writer | AC23 |
| U8 | Decided by Prashant Sharma, 2026-11-08: a person of a dedicated probe tenant in each environment; no demo data, left out of the demo screens, password a sealed secret (ADR 0006) | — | — | — |
| U14 | **Decided** by Prashant Sharma, 2026-11-08, as proposed; Gitea can do it (FACT, Gitea source, ADR 0005); the fallback is not needed. The question was: R2's condition lets only Prashant Sharma's and the `aidlc-agent` accounts push, but the design has three more writers: `jobwright-bot` (staging's digests on `jobwright-gitops` `main`, because 9.4 decides staging updates by itself), `renovate-bot` (update branches, 9.4), and the cluster's ledger writer (`release-ledger`). Also, ADR 0006's direct bot commit to `main` conflicts with ADR 0005's "no direct push". Proposed: each bot is a separate account with the narrowest right Gitea gives: `renovate-bot` pushes only `renovate/*` branches; the ledger writer pushes only `release-ledger`; `jobwright-bot` is allowed to push to `main` of `jobwright-gitops` only for `environments/staging/` (Gitea's unprotected file patterns: INFERENCE, to check against Gitea's documentation); none of them has cluster access. The alternative keeps the condition literally and makes staging a reviewed pull request, which changes Decided 9.4 | Prashant Sharma (risk owner) | Decision before design approval | ADR 0005, ADR 0006, AC23 |
| U9 | The measured idle memory | Prashant Sharma | AC28's script in slice 0 of `jobwright-gitops` | AC28, every later slice |
| U12 | Answered (FACT, docs.gitea.com v28): `permissions:` controls `GITEA_TOKEN`; `DEFAULT_ACTIONS_URL = github` (ADR 0005) | — | — | — |
| U13 | Does ArchUnitNET 0.13.5 run with xunit.v3 4.x? It restores; runtime support is not stated | Prashant Sharma | Smoke test in slice 0 of `jobwright-app`; fallback a reflection test with no library | ADR 0001's boundary check |
| U10 | Answered for create-next-app (FACT, nextjs.org create-next-app reference, `new-system.md` SCAFFOLD); shadcn and dotnet template flags are confirmed by slice 0's recipe commit, and a changed flag is a change to `new-system.md` | Prashant Sharma | Slice 0's recipe commit | Slice 0 of `jobwright-app` |

## Must-hold consequences and their checks

Each is a required check, drafted here and shown red once in its slice (the session at the root cannot run them).

| Must hold | Check | Repository | Criterion |
|---|---|---|---|
| The API matches its contract; the web matches the API | `ContractDocumentTests`; generated types and consumer tests; `oasdiff` | jobwright-app | section 8, AC4, AC13 |
| Isolation below the application | `RowLevelSecurityTests`; schema check (forced security, keys with `tenant_id`, no definer functions, `security_invoker` views, no BYPASSRLS on runtime roles) | jobwright-app | AC4, AC5 |
| Module boundaries | `Jobwright.ArchitectureTests` | jobwright-app | D1 |
| No public route to the API or worker | chart rendering test | jobwright-app | D1 |
| No readable token; headers on every response | AC14 and AC15 tests | jobwright-app | AC14, AC15 |
| Scripts refuse other clusters | guard test with a fake non-local context; lint that every cluster script sources the guard | both | AC22, E24 |
| Production only to proven versions | `checks/promotion` against the ledger | jobwright-gitops | AC23 |
| Pinned actions | workflow lint (40-character SHAs) | both | 9.4 |
| No job reaches the cluster | a CI job that fails if any API server answers from inside a job | both | D5 |
| Ports and memory | AC28's script | jobwright-gitops | AC28 |
| Schema changes are expand-only and reversible | AC25's job; up-down-up job | jobwright-app | AC25 |

Advisory, one line each: keep modules created only when an increment needs them (D1); prefer server components (D10);
keep Prometheus retention at 24 hours unless memory allows more (D7).

## Approval checklist (step 9)

| Item | State |
|---|---|
| Every decision has a Proposed record with a named decider | Yes: ADR drafts 1 to 10, decider Prashant Sharma |
| Contract changes follow the versioning rules, and consumers approved them | Contract v1 is new; its only consumer (the web app) has the same owner, who approves it with this design |
| Every non-functional requirement has a test, check or monitor | Yes for this increment's: `new-system.md` NFR CHECKS; increment 4's are named there as deferred (S11) |
| Every must-hold constraint is enforced, and its check was seen to fail | Enforced by the checks above; seen to fail: not yet (read-only session), each shown red in its slice |
| The map and rules file are updated | Route G: met by `new-system.md`, which `aidlc-onboard` turns into AGENTS.md and the map |
| Documentation updates named (public API or behaviour users see) | Each repository's README (run, test, the guarded scripts), `api/contract/openapi.yaml` as the API reference, and a CHANGELOG from the first release (writing-documentation) |
| No decision left for the build | Except U1 to U9, each with an owner and a fallback; none is for the build to make |
