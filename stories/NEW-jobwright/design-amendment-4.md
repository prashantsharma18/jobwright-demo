<!-- aidlc template: design-note -->
# NEW-jobwright · design note · amendment 4

## For the reviewer
- **Asked of:** Prashant Sharma, as tech lead, to approve the split of parts 3 and 4, the two test-only dependencies, the
  two CI changes, the 429 route and Released versions v2.1; as security owner, the STAGING_TOKEN hold (R-k) and risk
  R-n; as the consumer of Released versions (jobwright-gitops), v2.1
- **Record it:** On the APPROVED line below, and on this note's commit
- **Tier and main risk:** 3: STAGING_TOKEN usable before part 4's AppProject (R-k), or AC25's job testing something
  other than the versions that run (R-n)
- **Changed since the last version:** first version (2026-11-13), revised the same day after a fresh-context challenge
  (12 objections, all answered below in **Challenged**; the largest: part 4 splits into parts 4 and 13, which removes
  a wait cycle with part 12, and STAGING_TOKEN is deleted until part 12)

Inputs: [spec v4](spec.md), the approved design with amendments 1 to 3, ADRs 0001 to 0015, and `jobwright-app`'s part 3
plan v1 (approved by Prashant Sharma, 2026-11-13) with its proposal to this root,
`plans/NEW-jobwright/part-3/root-proposal.md`, both on branch `feat/NEW-jobwright-part-3` at dbbffae. Plan K1, K5, K8,
Q1, Q2, Q3 and Q5 and challenge Y1 and Y11 are the sources. No criterion changes, so the spec stays v4. Jobwright API
v1 and Release ledger v2 stand; Released versions gains v2.1 (D6, additive). No new ADR: nothing here changes a
decision an ADR records. Everything else in the approved design stands.

## Design note: the three questions
- Does it alter the system's shape? No: the same two repositories, services and boundaries.
- Does it move a boundary? No.
- Does it change a contract someone outside the team relies on? Yes, additively: Released versions v2.1 (D6) says what
  the first staging run does when the values file is absent. D5 also fixes one web path that part 4's Traefik
  configuration names, recorded in the coordination plan.

## Decisions

| Item | Change |
|---|---|
| D1 · Split parts 3 and 4 (plan K1, Q3; challenge 1) | Part 3 keeps its first pull request (P1). P2 to P9 become parts 5 to 12 in `jobwright-app`. Part 4 in `jobwright-gitops` keeps the platform (Traefik, Rollouts, Prometheus, Keycloak, the AppProject); its release slices become part 13, which waits for part 12's first release. Each part has its own branch `feat/NEW-jobwright-part-<n>`, index `plans/NEW-jobwright/part-<n>/README.md` and plan |
| D2 · Microsoft.OpenApi.YamlReader 2.12.2 (plan Q1) | Approved for `jobwright-app`'s contract test project only, never in a project under `api/src`; it brings SharpYaml 2.1.5. ContractDocumentTests (ADR 0004) read `api/contract/openapi.yaml` with it. Needed by part 6 |
| D3 · Mailpit as a test container (plan Q2) | `axllent/mailpit:v1.33.1` may run as a Testcontainers container in `jobwright-app`'s AC29 integration test (part 10), pinned there by digest, which part 10 records. A test image only, never deployed from `jobwright-app` |
| D4a · AC25's job (plan K5, challenge Y11; challenges 4, 5) | On every pull request, from part 5 on, in the `integration` job: start PostgreSQL 17, apply the branch's migrations, then run `main`'s head `*.IntegrationTests` against that database, with the fixture's own migration step off. The fixture reads `JOBWRIGHT_TEST_DATABASE` (a connection string); when it is set, the fixture uses that database as it is and migrates nothing. Part 5 builds the switch. No path filter, so an edited migration or a raw-SQL change to roles or row-level security is caught too. Replaces "AC25 from the second release" in `new-system.md` CI; oasdiff stays "from the second release" |
| D4b · Gates by project (plan challenge Y1; challenge 6) | `*.IntegrationTests` and `*.ContractTests` run in the `integration` job, and every other .NET test project in `test`. The web consumer tests run in `test` (vitest), and oasdiff runs in `integration` from the second release. `scripts/tests/test_check_split.sh`, built in part 3's integration-gate slice, fails when a test project is in no gate or in two, and when a project that runs in `test` references `Testcontainers.*`. Replaces v3's `--filter-trait "Category=..."`; `new-system.md`'s separate "contract" gate becomes these steps of `test` and `integration` (the required checks stay setup, lint, test, build, browser, budget, integration) |
| D5 · The sign-in 429 as a problem response (plan Q5; ADR 0008; challenges 8, 9, 10) | ADR 0008 decides the web app's sign-in refusals are problem responses with Retry-After. How: part 4's Traefik slice puts an errors middleware (status 429, the web service, query `/errors/rate-limited`, `errorRequestHeaders` empty) before the rate-limit middleware on the web app's `/auth/*` routes. Part 7 builds `GET /errors/rate-limited`: outside the session middleware (it reads no cookie, refreshes no session, never redirects), reads no request input, and answers 429, `application/problem+json`, type `https://jobwright.example/problems/rate-limited` (ADR 0004's closed list), `Retry-After: 60`, `Cache-Control: no-store`. 60 is E9's window: never less than the limiter's real wait. Part 4's Traefik test asserts the limit's period is 60 s or less, so the two stay consistent. Keycloak's routes keep Keycloak's own page (ADR 0008) |
| D6 · Released versions v2.1 (challenge 7) | When `environments/staging/values.yaml` is absent in `jobwright-gitops`, the release run creates it, as if `runNumber` were 0, with the same content and commit message; nothing else changes. Additive. Provider test drafted in `contract/tests/released-versions/test_staging_commit.py`; the consumer's values check accepts a first file with no previous one (part 4 adds that test before part 12 runs). FACT: `environments/` does not exist on `jobwright-gitops` `main` today (`git ls-tree`, 2026-11-13) |
| D7 · STAGING_TOKEN held back (R-k; challenge 2) | Prashant Sharma deletes STAGING_TOKEN from `staging-release` now, if it exists (decisions-2026-11-11 had him create it after amendment 2), and creates it again only when part 12's plan is approved with its three preconditions met. With no secret, no job in parts 3 and 5 to 11 can write `jobwright-gitops`, whatever a workflow names. This replaces the person-only check of the first draft |

### D1 · the parts

| Part | Repository | Was | What | Proves | Waits for |
|---|---|---|---|---|---|
| 3 | app | P1 | contract, problems and headers, health and version | AC13, AC15, AC17, AC18 (version), AC27 (part: the split check and the `integration` job) | parts 1, 2 |
| 4 | gitops | part 4 | the platform: Traefik, Rollouts, Prometheus, Keycloak, the AppProject | AC16 (the limit), AC22, AC23 (checks/promotion with fixture pull requests), AC24, AC27, AC28 | part 2 |
| 5 | app | P2 | isolation in the database | AC4 (keys), AC5, AC25, NFR-DATA-02 | 3 |
| 6 | app | P3 | API authorization and the v1 operations | AC4, AC6 (API), AC13, E2, E5, E20 | 5 |
| 7 | app | P4 | sign-in and sessions | AC1 to AC3, AC14, AC16 (app share; the 429 route, D5), E1, E3, E4, E6 to E9, E12, E18, E25 | 6 |
| 8 | app | P5 | the shell and accessibility | AC6 (offered), AC7 to AC12, AC26 on the Overview, E10, E11, E19 | 7 |
| 9 | app | P6 | telemetry | AC18 (telemetry), AC19, E16, E26 | 3 |
| 10 | app | P7 | the tenant command | AC29, E27 | 5 |
| 11 | app | P8 | gates, images, chart | AC17 (probes), AC22, AC24, AC27 (gates drill), E23, E28, E33 | 3 |
| 12 | app | P9 | the release job | Released versions v2.1 (provider), E28, E31, E33 (release side) | 3, 5 to 11; part 4's AppProject merged; `jobwright-gitops`'s NEW-slice0-review-fixes merged with its security gate given |
| 13 | gitops | part 4's release slices | staging's Application, the release drill, AC16 end to end | AC20, AC21, AC16 (through Traefik: U16) | part 4; part 12's first release |

The `jobwright-app` rows cover what part 3 proved under v4 (AC1 to AC19, AC22, AC24 to AC27, AC29) and every E-item
part 3's plan v1 assigns in its Criteria coverage (E1 to E12, E16, E18 to E20, E23, E25 to E28, E31, E33): FACT, their
union checked against plan.md's table. Parts 4 and 13 together cover v4's part 4 (AC16, AC20 to AC24, AC27, AC28).
Each part's own `aidlc-plan` rates its slices' tiers (part 3 plan K9, Y12) and writes its bundles, and each slice runs
`aidlc-review` before its pull request merges (part 3 plan K10).

**Options for D1.**
- **A, chosen: one part per pull request in `jobwright-app`, and part 4 split at its release slices.** Each part has
  one index and one branch, so `story_status.py` and TRACKING work unchanged. Every wait names a part that can merge
  on its own: part 12 waits for part 4, and part 13 waits for part 12, with no cycle.
- **B, the cheapest: keep one part 3, with a pull request per slice on stacked branches.** No root change. Against it:
  the status script reads one index per part, so part 3 would read "merged" only after the ninth pull request, and
  the root could not see which groups had merged. The same objection rules out keeping part 4 whole while part 12
  waits on one of its slices (challenge 1).
- **C: one part 3 as one pull request.** About 8,000 changed lines (part 3 plan K1, INFERENCE from its slice map),
  far past the roughly 1,000 that reviewing-code accepts.
- **Strongest argument against A:** ten plans, ten reviews and ten merges for one person (part 3 plan R-h). Accepted:
  the plans are short because part 3's plan is their slice map, and Tier 1 slices run delegated (K9).

**Options for D2.**
- **A, chosen: Microsoft.OpenApi.YamlReader 2.12.2.** It is Microsoft's own reader for the Microsoft.OpenApi already in
  the graph. FACT: `Jobwright.Api`'s lock file resolves Microsoft.OpenApi 2.12.2 under [2.12.2, 3.0.0). FACT: its
  api.nuget.org nuspec, read 2026-11-13, gives authors Microsoft, licence MIT, and dependencies Microsoft.OpenApi 2.12.2
  and SharpYaml 2.1.5. SharpYaml 2.1.5's nuspec gives Alexandre Mutel, MIT. The newest YamlReader, 3.11.0, needs
  Microsoft.OpenApi 3.11.0, outside the ASP.NET Core range, so YamlReader moves only in step with Microsoft.OpenApi.
- **B, the cheapest: a JSON copy of the contract.** Against it: ADR 0004 names only `openapi.yaml`, and two copies
  can drift.
- **Against A:** a third-party YAML parser enters the graph. Bounded: a test project only, with its lock file scanned
  (trivy fs, NFR-SEC-07), and part 6 adds a check that no `api/src` project references either package.

**Options for D3.** A, chosen: the pinned image in the test, so AC29 proves the invitation reached a mailbox. B, the
cheapest: none, so the test proves only Keycloak's admin event. Against A: one more container (R-e, memory). Bounded:
only part 10's test runs it.

**Options for D4a.** AC25 says "the previous released version". Every merge to `main` releases to staging (ADR 0012).
- **A, chosen: `main`'s head**, readable by a pull-request job with no secret. That matters on a public repository,
  where fork runs get none.
- **B: the `revision` in staging's values file** (Released versions v2). Exact, but it lives in the private
  `jobwright-gitops`, so a pull-request job would need a read token.
- **C, the cheapest: v4's "from the second release".** Parts 5 and 6's schema changes would go untested.
- **Risk R-n, restated after challenge 4.** The versions that run can be older than `main`'s head: production
  routinely is (promotion by review, ADR 0012), and staging is after an abort. Branches need not be up to date
  (ADR 0014), so no pull request is guaranteed to have tested an older version against the newest schema. The bound
  therefore rests on one rule alone: migrations only expand (ADR 0006), checked by the up-down-up job and by review.
  ADR 0006 allows a later contraction. Increment 1 contracts nothing (INFERENCE: a new system with no old shape to
  remove). **The first contraction returns to `aidlc-design`** to extend AC25's job to the revision production runs.
  Owner Prashant Sharma.
- Spec wording: "the previous released version" is read as `main`'s head. If Prashant Sharma reads it as "the last
  release tag", that is a criterion change for `aidlc-specify`.

**Options for D4b.**
- **A, chosen: by project**, enforced by the split check. The check includes the Testcontainers rule, because the
  runner has Docker in every job and an integration test misplaced in `test` would otherwise pass there unnoticed
  (challenge 6, FACT: bundle-integration-gate.md, no services).
- **B, the cheapest: keep traits, add `--ignore-exit-code 8`.** Microsoft Testing Platform exits 8 when no test runs
  (part 3 plan challenge Y1). Against it: the approved tests carry no trait, so they would run in no gate, and ignoring
  exit 8 also hides a project that lost every test.

**Options for D5.**
- **A, chosen: Traefik's errors middleware forwards to a web route.** Traefik documents the errors middleware's
  `status`, `service`, `query` and `errorRequestHeaders` (doc.traefik.io, reference, HTTP middlewares, Errors, read
  2026-11-13; the page is the current reference, not pinned to 3.7.17: INFERENCE that it applies).
- **B, the cheapest: Traefik's own 429 body.** Against it: AC16 asks for a problem response, and ADR 0008 decided the
  web app gives it.
- **C: the web app counts attempts itself.** Against it: two pods do not share counters, and it duplicates E9.
- **Against A:** whether the route's Content-Type and Retry-After reach the client through Traefik is undocumented
  (U16, restated after challenge 8). It can be shown only end to end, once part 7's route is deployed, so the proof
  sits in part 13.
- **Note for part 4 (challenge 9):** Traefik's limiter is a token bucket (`average`, `period`, `burst`), so after a
  refusal a new attempt may pass after about 6 s, not after "the rest of the minute" as E9 reads. `Retry-After: 60` is
  an honest upper limit. Part 4's plan says how E9's wording maps to `average` and `burst`.

**D6 options.** A, chosen: the first run creates the file (provider). B: part 4 commits a starting file. Against B:
the values check refuses a file without three digests of one revision, and there are no images before part 12.
Against A: a contract change. Bounded: it is additive, and the same person owns both ends.

**D7 options.** A, chosen: no secret until part 12. B: a CI lint that fails any workflow naming `staging-release`
before part 12 (challenge 2's other fix). Against B: part 12 must switch it off, and it guards only what it parses.
Against A: Prashant Sharma has to create the token again, a few minutes' work, and it gets a fresh expiry.

## Must-hold constraints and their checks

| Constraint | Check | Where | Seen red |
|---|---|---|---|
| No job writes `jobwright-gitops` before part 12's preconditions | STAGING_TOKEN absent (D7); the environment is `main`-only (ADR 0014) | Prashant Sharma, GitHub settings | a person's check: he confirms the secret list of `staging-release` is empty, recorded in the story index |
| Each test project is in exactly one .NET gate, and only integration or contract projects use Testcontainers | `scripts/tests/test_check_split.sh` | `jobwright-app`, part 3's integration-gate slice | at its first step, with a project in no gate, one in two, and a unit project referencing Testcontainers |
| Schema changes do not break `main`'s head | AC25's job (D4a) | `jobwright-app`, part 5 | at part 5's second migration, with a planted dropped column |
| YamlReader and SharpYaml never ship | No `api/src` project references them | `jobwright-app`, part 6's contract-tests slice | at its first step, with a planted reference |
| The 429 reaches the client as a problem response with Retry-After | the 11th attempt through Traefik (U16) | `jobwright-gitops`, part 13 | in that slice, before the route is wired |
| The limit's period is never longer than the route's Retry-After | a test on the rendered middleware | `jobwright-gitops`, part 4's Traefik slice | in that slice |
| The first staging run creates the values file | `test_released_versions_v2_1_an_absent_file_is_created_as_run_zero` | `jobwright-app`, part 12 | at part 12's first step |

This session at the workspace root is read-only and runs no tests: each check is drafted here and shown red at the
first step of its slice, as the table says.

## Unknowns

| ID | Question | Owner | Answered in | Blocks |
|---|---|---|---|---|
| U16 | Does the 11th sign-in attempt through Traefik return 429 with `application/problem+json` and a Retry-After from `/errors/rate-limited`? | Prashant Sharma | part 13, end to end (needs part 7's route deployed by part 12) | AC16's end-to-end evidence only |

## Threat model, changed rows

| Boundary | Threats | Mitigations |
|---|---|---|
| Test dependencies -> CI runner (`jobwright-app`) | T: a compromised SharpYaml or Mailpit image runs code in CI | Test projects only; lock files pinned and scanned (trivy fs, NFR-SEC-07); Mailpit pinned by digest in part 10; pull-request jobs hold no secret (ADR 0014) |
| Client -> Traefik -> web `/errors/rate-limited` | D: the route used to get around the limit; I: the route echoing request data; T: a session refreshed by a forwarded cookie | The route reads no input and no cookie, sits outside the session middleware, returns a fixed body with `no-store`, and does nothing a sign-in does; `errorRequestHeaders` is empty (D5) |
| `jobwright-app` CI -> `jobwright-gitops` `main` | T, E: a job writing staging, and through Argo CD the cluster, before the AppProject (R-k) | D7: no STAGING_TOKEN until part 12, which waits for part 4's AppProject and the review fixes' security gate |

## Challenged

A fresh-context reviewer argued against the first draft on 2026-11-13 (stress-testing-decisions). Each objection, and
what changed:

| # | Objection | Answer |
|---|---|---|
| 1 | Part 4 waits for part 12's release, and part 12 for a slice inside part 4: a cycle the status script cannot resolve (blocker) | Fixed: part 4 splits; part 13 takes the release slices (D1) |
| 2 | Only a person stops STAGING_TOKEN being used early, and the secret probably exists | Fixed: D7, the secret is deleted until part 12 |
| 3 | E9, E18, E23, E28, E31, E33 and "AC27 (part)" fell out of the rows | Fixed: rows completed; the union check covers the E-items |
| 4 | R-n's bound was overstated (production runs older versions; branches need not be up to date; ADR 0006 allows contraction) | Fixed: R-n restated; the first contraction returns to aidlc-design |
| 5 | AC25's mechanism was left to the build | Fixed: D4a fixes the steps, the fixture switch and the trigger |
| 6 | Testcontainers runs in any job; contract tests' gate disagreed with part 3's bundle; the split check's slice was wrong | Fixed: D4b aligns with part 3's bundle and adds the Testcontainers rule |
| 7 | No one creates `environments/staging/values.yaml` before the first release | Fixed: D6, Released versions v2.1 |
| 8 | U16 asked the wrong question and had no end-to-end proof | Fixed: U16 restated; proof in part 13 |
| 9 | A fixed 60 s versus a token bucket | Accepted as an honest upper limit; part 4 asserts the period and maps E9 (D5) |
| 10 | The route's session, method and cache behaviour were left to the build | Fixed: D5 decides them |
| 11 | Mailpit pinned by tag while the threat model said digest | Fixed: part 10 pins it by digest |
| 12 | The review fixes and security gate are not PARTS rows; `--check-release` would contradict part 12's staging releases | Fixed in ORDER: they are named manual gates, and `--check-release` governs `aidlc-release` (production), not the staging release every merge makes after part 12 |

## What changes

| File | Change |
|---|---|
| [coordination-plan.md](coordination-plan.md) v5 | PARTS: parts 5 to 13; part 3 and part 4 narrowed; TRACKING; OUTSIDE (D7); ORDER (manual gates, `--check-release`); VERIFY (AC25); UNKNOWNS (U16); ROLLBACK; CONTRACTS (Released versions v2.1) |
| [new-system.md](new-system.md) v9 | `jobwright-app` DEPENDENCIES (YamlReader, Mailpit in tests); CI (gates by project; AC25 as D4a); Pinned versions |
| [contract/released-versions-v2.md](contract/released-versions-v2.md) | v2.1 section (D6) |
| [contract/tests/released-versions/test_staging_commit.py](contract/tests/released-versions/test_staging_commit.py) | the absent-file test (D6) |
| [README.md](README.md) | this stage's row; CURRENT gains Released versions v2.1 on approval |
| `jobwright-app` (not written from here) | Part 3's plan and index: P2 to P9 renamed parts 5 to 12; the integration-gate bundle's split check gains the Testcontainers rule (D4b) |
| `jobwright-gitops` (not written from here) | Part 4's plan: the platform only, with D5's errors middleware, the period test, E9's mapping and D6's consumer test; part 13's plan later |

APPROVED     Prashant Sharma (tech lead; security owner for R-k and R-n; consumer of Released versions), 2026-11-14
             ("I approve design amendment 4"), recorded by Claude at his instruction: D1 to D7, `new-system.md` v9,
             coordination plan v5 and Released versions v2.1
