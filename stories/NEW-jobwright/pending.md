# Jobwright demo: every pending item, with the facts the parallel-lanes coordinator reads

Read-only inventory, 2026-11-22. Nothing was changed, staged, committed, stashed or checked out in
`the workspace root` or its two repositories. No cluster command, no push, no `gh` write. `gh` was used
read-only three times (secret names, required checks, open PRs, one PR comment).

This is a list, not a schedule. It does not decide what runs in parallel. That is the coordinator's job.

Path shorthand used below:
- `ROOT` = `the workspace root`
- `APP` = `ROOT/jobwright-app`
- `GITOPS` = `ROOT/jobwright-gitops`
- `CP` = `ROOT/stories/NEW-jobwright/coordination-plan.md` (v5)
- `DA4` = `ROOT/stories/NEW-jobwright/design-amendment-4.md`
- `P3` = `APP/plans/NEW-jobwright/part-3/` (on branch `feat/NEW-jobwright-part-3` only)
- `RR2` = `GITOPS/plans/NEW-jobwright/part-2/review-record.md`
- `AIDLC` = `the aidlc plugin` (8.3.0, `.claude-plugin/plugin.json:3`)

The coordinator reads these facts for each row (`AIDLC/skills/parallel-lanes/SKILL.md:31-37, 62-69`;
`AIDLC/skills/parallel-lanes/assets/queue.md:18-44`; `AIDLC/skills/lane-run/SKILL.md:40-66`):
- the row, its repository, its branch and what it depends on;
- its state (waiting, stopped, waiting for push and merge, done, ready), read from the row's own index and progress note;
- its next stage, its tier and its bundle MODE, which decide whether a sub-agent may run it;
- for the clash table: the order, MAY CHANGE paths shared with a building lane, an API contract another open row
  changes, and a shared fact.

---

## 1. In flight

### IF-1 · NEW-jobwright part 3 · contract, problems, headers, health (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Queue row | `NEW-jobwright part 3` | `CP:22`; row form from `AIDLC/skills/parallel-lanes/assets/queue.md:33` |
| Repository | jobwright-app | `CP:22` |
| Branch | `feat/NEW-jobwright-part-3` at `9eed7af`, 29 commits ahead of `main`. Local only: no upstream and no `origin/feat/NEW-jobwright-part-3` | `git -C APP branch -a -vv`; `git -C APP rev-list --count main..HEAD` = 29 |
| Pushed? | No | the same; `gh pr list --state open`: no open PR |
| Checked out where | In the main working copy `APP` itself (the only worktree) | `git -C APP worktree list` |
| Tier | 3 for the part; each slice rated on its own | `P3/plan.md:11-13`, `P3/plan.md:71-80` (K9) |
| Index state | aidlc-plan `done`. aidlc-build `doing (contract done …; api-problems-headers done …; web-headers done …, exception E-1 for the pull request; health-version next)`. aidlc-verify, aidlc-review and merged `to do` | `P3/README.md:19-23` |
| Index stale lines | `LEVEL part 3 of 4` (13 parts now). `SOURCE … coordination plan v4` (v5 now). `PINS spec v4 · plan v1 · contract jobwright-api v1` (no home commit) | `P3/README.md:6, 9, 10` |
| Plan on `main`? | No. `origin/main` holds only `plans/NEW-jobwright/part-1` | `git -C APP ls-tree --name-only origin/main plans/NEW-jobwright/` |
| Slices (all on the one branch, in order) | contract (T2, hands-on, done) → api-problems-headers (T3, hands-on, done) → web-headers (T3, hands-on, done) → health-version (T2, delegate with checkpoints, **paused**) → integration-gate (T3, hands-on, **not started**) | `P3/README.md:39-45`; `P3/progress-contract.md:9`, `progress-api-problems-headers.md:9`, `progress-web-headers.md:9` (`STATUS done`) |
| Independent slices? | No. No slice has its own branch and no bundle has a `DEPENDS ON` line. Each slice waits for the one before it, so for the coordinator part 3 is one row, not a row per slice | `P3/README.md:39-45` ("Waits for" column); `grep "DEPENDS ON" P3/bundle-*.md`: no hit; rule in `AIDLC/skills/parallel-lanes/SKILL.md:35` |
| Paths the branch has changed so far (outside `plans/`) | `api/contract/openapi.yaml`, `api/src/Jobwright.Api/Http/{Problems,SecurityHeaders}.cs`, `api/src/Jobwright.Api/Program.cs`, `api/tests/Jobwright.Api.Tests/{ProblemResponseTests,ProblemTypesTests,SecurityHeaderTests}.cs`, `api/tests/Jobwright.Api.Tests/TestHost/FailingApiFactory.cs`, `scripts/check.sh`, `web/.prettierignore`, `web/next.config.ts`, `web/package.json`, `web/pnpm-lock.yaml`, `web/src/app/{layout,not-found}.tsx`, `web/src/lib/csp{,.test}.ts`, `web/src/proxy.ts`, `web/src/server/api/schema.d.ts`, `web/tests/e2e/headers.spec.ts` | `git -C APP diff --name-only main...HEAD` |
| The union of the part's files | `P3/plan.md:149-176` (FILES) |  |
| Contract | Lands Jobwright API v1 (`1.0.0-draft.1`) byte for byte. A later change goes through aidlc-design only | `P3/plan.md:65-67` (K7); `CP:12, 77` |
| Next stage | aidlc-build: resume health-version at step 3, then integration-gate, then aidlc-verify, aidlc-review, merged | `P3/README.md:20-23` |
| Depends on | parts 1 and 2. Both are merged | `CP:22`; root index `ROOT/stories/NEW-jobwright/README.md:33, 40` |

Blockers for part 3 as a whole:
- **The plan is not on `main` (8.3.0).** `aidlc-build` stops at Tier 2 or 3 when "the bundle is missing from the
  default branch or differs from it" (`AIDLC/skills/aidlc-build/SKILL.md:60`, and line 37: "At Tier 2 and 3, the bundle
  is the copy merged on the default branch"). Part 3's plan and five bundles exist only on the unpushed feature branch
  (5.0.0 practice). A plan pull request must merge first. Only a person can push it here (see the push rule below).
- **The branch is checked out in the main working copy.** A lane claims with `git worktree add .worktrees/<folder> <branch>`.
  That fails with `is already checked out`, which a lane reads as "another lane holds the row"
  (`AIDLC/skills/lane-run/SKILL.md:89-92`). The parallel-lanes rule is that "a person does not check out a queued
  branch in their own working copy while lanes may claim it" (`AIDLC/skills/parallel-lanes/SKILL.md:183`). The 25
  uncommitted entries (IF-1a) sit in that working copy.
- **The part-path gap.** 8.3.0 keeps the first part in a repository in `plans/<KEY>/` and a second part in
  `plans/<KEY>/part-<n>/` (`AIDLC/skills/aidlc-build/SKILL.md:37`). Its worktree folder is `.worktrees/<KEY>-p<n>`
  (`AIDLC/skills/lane-run/SKILL.md:63-66`). But lane-run's state table reads `done` from `plans/<KEY>/README.md`
  (`AIDLC/skills/lane-run/SKILL.md:47`), and `story-run` reads `plans/<KEY>/README.md` (`AIDLC/skills/story-run/SKILL.md:25, 54`).
  This demo keeps every part in `part-<n>/`, part 1 included (`CP:46-48`). So a lane finds no index at
  `plans/NEW-jobwright/README.md` in jobwright-app. 8.3.0 names no `part <n>` argument for story-run (`SURVEY:269-272`).
- **`.worktrees/` is not ignored.** `git check-ignore -q .worktrees/` fails at the root and in both repositories. lane-run
  stops on that (`AIDLC/skills/lane-run/SKILL.md:75-76, 200`). Command run here: `git -C <repo> check-ignore -q .worktrees/`
  → not ignored in all three.
- **Agents may not push.** "Agents never push; Prashant Sharma pushes (spec v2, A2)" (`APP/AGENTS.md:119`); settings deny
  `Bash(git push*)` (`APP/.claude/settings.json:16`). So every lane releases with `waiting: push and open the pull
  request by <who>` (`AIDLC/skills/lane-run/SKILL.md:139-141`).
- **Part 3's progress notes use the 5.0.0 format** (free bullets, no decision-log rows with commit and owner;
  "Context gaps" sections). 8.3.0's review copies decision-log rows by owner (`SURVEY:301-304`, `:256`).

#### IF-1a · part 3, slice health-version (paused)

| Fact | Value | Source |
|---|---|---|
| State | `STATUS paused: step 3, the integration and browser runs wait for the k3d cluster to be stopped`. lane-run counts `paused: …` as waiting | `P3/progress-health-version.md:9`; `AIDLC/skills/lane-run/SKILL.md:44` |
| MODE / tier | delegate with checkpoints / Tier 2 | `P3/bundle-health-version.md:21-22`; `P3/progress-health-version.md:10` |
| Start commit | `9edaa54` | `P3/progress-health-version.md:11` |
| Steps done | 0, 0b, 1, 2 (tests written, seen red) | `P3/progress-health-version.md:43-73` |
| Step 3 | `[~]` code written, **not committed**. Green: web 19 of 19; Api.Tests 36, Worker.Tests 3, ArchitectureTests 1. IntegrationTests builds, never run (8 ReadinessTests cases). headers.spec.ts in the pinned image not run | `P3/progress-health-version.md:74-93` |
| Steps open | 4 (full checks), 5 (boundary and self-review), 6 (STATUS done, index row) | `P3/progress-health-version.md:94-98` |
| Commit rule | Tests and code are committed together once the integration tests are green. Nothing of steps 2 and 3 is committed: `git diff --name-only 9edaa54..HEAD` lists only the progress note | `P3/progress-health-version.md:56-57, 78`; command run here |
| Uncommitted work | 14 tracked paths modified or deleted (+383 −598), plus untracked files: 25 entries in `git status --short`, 39 paths with `-uall`. All inside MAY CHANGE except `.claude/settings.json`, which is the owner's sandbox-off edit and is never staged (K12) | `git -C APP status --short`, `git -C APP status --porcelain -uall | wc -l` = 39, `git -C APP diff --stat`; `P3/plan.md:89-90` (K12); `P3/progress-health-version.md:45-46` |
| MAY CHANGE | `api/src/Jobwright.Api/Program.cs`, `api/src/Jobwright.Api/Health/**`, `api/src/Jobwright.Api/Jobwright.Api.csproj`, `api/src/Jobwright.Worker/**`, `api/src/Jobwright.SharedKernel/**`, `api/tests/Jobwright.Worker.Tests/**`, `api/tests/Jobwright.IntegrationTests/**`, `api/tests/Jobwright.Api.Tests/Version*.cs`, `api/Jobwright.slnx`, `api/**/packages.lock.json`, `web/src/app/health/**`, `web/src/app/version/**`, `web/src/server/version.ts`, `web/src/server/version.test.ts`, `web/next.config.ts`, `web/tests/e2e/headers.spec.ts`, `plans/NEW-jobwright/part-3/**` | `P3/bundle-health-version.md:38-43` |
| Next step | Resume step 3: run `dotnet test --project api/tests/Jobwright.IntegrationTests` and headers.spec.ts in the pinned image, then commit tests and code together, then steps 4 to 6 | `P3/progress-health-version.md:92-98` |
| Depends on | web-headers (done) | `P3/README.md:44` |
| Blocker: k3d | Integration tests need Docker with the k3d cluster `jobwright` stopped, and agents never run k3d. The owner said "k3d is stopped" at checkpoint 2 (2026-11-14), but the cluster was running. The note asks him to stop it, "or say they may run while it runs" | `P3/progress-health-version.md:36-40, 176-177, 204-210` |
| Blocker: a person's word | The wait needs the owner's word or action (k3d), so under 8.3.0 a person starts the lane by hand, not a sub-agent | `AIDLC/skills/parallel-lanes/SKILL.md:86`; `AIDLC/skills/lane-run/SKILL.md:105-110` |
| Known effect on CI | Until integration-gate lands, `scripts/check.sh build` and CI's build job fail on this branch, because the image builds have no `.git` and the host projects now refuse a build without a 40-hex commit (C5) | `P3/progress-health-version.md:164-170` |
| Records format | "Skills loaded" says plugin version 5.0.0 | `P3/progress-health-version.md:179-182` |

#### IF-1b · part 3, stages after build (verify, review, merged)

| Fact | Value | Source |
|---|---|---|
| State | `to do` | `P3/README.md:21-23` |
| Must happen at or before merge | `integration` added to the required checks on `main` (Q4). Today the rule lists only setup, lint, test, build, browser, budget | `P3/plan.md:258-259, 281`; `CP:72`; `gh api …/branches/main/protection/required_status_checks --jq .contexts` → `["setup","lint","test","build","browser","budget"]` |
| Must be restated in the PR for the owner to confirm | Exception X-1 (`P3/plan.md:96-102, 279-280`). Exception E-1 to AC15 (`P3/progress-web-headers.md:233-237`). Web-headers' C4 and Option A (`P3/progress-web-headers.md:226-231`) | as cited |
| Size trigger | If aidlc-verify counts more than 1,000 hand-written lines, part 3 splits before review: integration-gate becomes its own pull request | `P3/plan.md:218-220` (R-l) |
| Review rule | Every slice runs aidlc-review before the pull request merges (K10) | `P3/plan.md:81-83`; `P3/README.md:47` |
| Merge order | In jobwright-app, one pull request at a time; part 3 first | `CP:79-80` |

### IF-2 · NEW-slice0-review-fixes release record (jobwright-gitops)

| Fact | Value | Source |
|---|---|---|
| What | The release record of jobwright-gitops's own story NEW-slice0-review-fixes (route D2, Tier 3). It is a follow-up to part 2, not a NEW-jobwright part | `GITOPS/plans/NEW-slice0-review-fixes/README.md:1-11` |
| Branch | `chore/NEW-slice0-review-fixes-release` at `afecd59`, 1 commit ahead of `main`, no upstream: never pushed | `git -C GITOPS branch -a -vv`; `git -C GITOPS rev-list --count main..HEAD` = 1 |
| Checked out where | The main working copy `GITOPS` (tree clean) | `git -C GITOPS worktree list`; `git -C GITOPS status --short`: empty |
| Paths it changes | `plans/NEW-slice0-review-fixes/README.md` (the aidlc-release row only), `plans/NEW-slice0-review-fixes/release.md` (new) | `git -C GITOPS show --stat HEAD` |
| State | On the branch, aidlc-release reads `done (2026-11-14, release.md …)`. On `main` it reads `to do`. `release.md` is not on `main` | `git -C GITOPS diff main HEAD -- plans/NEW-slice0-review-fixes/README.md`; `git show main:plans/NEW-slice0-review-fixes/release.md` fails |
| Next step | A person pushes it and opens and merges a docs pull request, or decides to leave it local and record that | `GITOPS/AGENTS.md:136`; `SURVEY:322-323` (H4) |
| Depends on | Nothing. Every other row of that story is done on `main`, merged row included (PR #4, `37639d2`) | `GITOPS/plans/NEW-slice0-review-fixes/README.md:20-29` |
| Blockers | Agents may not push (`GITOPS/AGENTS.md:136`; `GITOPS/.claude/settings.json:16`). `.worktrees/` not ignored (command above) |  |
| What it carries forward | "Open after the release": M4 and L4 before part 3's release job holds STAGING_TOKEN; the owner's actions M9, M10, question 6, L11, questions 2, 4, 5; later stories | `GITOPS/plans/NEW-slice0-review-fixes/release.md:29-33` |

No other work is started but unfinished. Checked:
- No branch in either repository holds `plans/NEW-jobwright/part-<4..13>/`, a `slice-<n>/` index or a `QUEUE.md` (loop over every local and remote branch with `git ls-tree -r`: no hit).
- No open pull request in either repository (`gh pr list --state open`: empty for both).
- The root has no unmerged branch (`git -C ROOT branch -a`; `SURVEY:56-58`).

---

## 2. Not started

Every remaining part has **no index, no plan and no bundle** on any branch (the `git ls-tree` loop above). Under
8.3.0 "a row whose item has no index yet is not ready: a person routes it with `aidlc-start` first"
(`AIDLC/skills/lane-run/SKILL.md:53`). TRACKING says each of parts 5 to 12 "gets its index, plan and bundles from its
own aidlc-plan, from part 3's plan v1 slice map" (`CP:49`). ORDER says "Each part's aidlc-plan starts after the part it
waits for has merged" (`CP:81`). Every part names Prashant Sharma as owner (`CP:20-32`). The branch for each is
`feat/NEW-jobwright-part-<n>` and the index `plans/NEW-jobwright/part-<n>/README.md` (`CP:46-48`; `DA4:33`).

Tiers for parts 5 to 12 are provisional; "each part's own plan rates them" (`P3/plan.md:79-80`; `DA4:61`).

### NS-3b · part 3, slice integration-gate (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| What | AC27 (part): the `integration` gate, tests split by project, the images carry their commit | `P3/bundle-integration-gate.md:14-16` |
| Tier / MODE | 3 / hands-on | `P3/bundle-integration-gate.md:20-21` |
| Waits for | health-version; Q4 before merge | `P3/README.md:45` |
| MAY CHANGE | `scripts/check.sh`, `scripts/tests/**`, `.github/workflows/ci.yml`, `Dockerfile.web`, `Dockerfile.api`, `Dockerfile.worker`, `plans/NEW-jobwright/part-3/**` | `P3/bundle-integration-gate.md:35-36` |
| Plan | Bundle v1 exists, approved 2026-11-13 on the feature branch only | `P3/plan.md:274-281` |
| Pending change to the bundle | Design amendment 4 says "the integration-gate bundle's split check gains the Testcontainers rule (D4b)" (`DA4:196`; the rule at `DA4:37`). The bundle's step 1 does not have it (`P3/bundle-integration-gate.md:55-57`). The bundle says "a changed bundle is approved again" (`:5`). So this needs a revised bundle and a new approval. (Rule read from the files; whether a re-plan was meant is UNKNOWN) | as cited |
| Contract / shared fact | None. It adds a required check (ADR 0014) | `P3/bundle-integration-gate.md:51-52` |

### NS-4 · part 4 · the platform: Traefik, Rollouts, Prometheus, Keycloak, the AppProject (jobwright-gitops)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 2` (merged) | `CP:23`; root index `:40` |
| Parallel note in the records | "(v5) Part 4 (the platform, with the AppProject) may run alongside parts 3 and 5 to 11, and merges before part 12" | `CP:82` |
| Proves | AC16 (the limit), AC22, AC23 (checks/promotion with fixture pull requests), AC24, AC27, AC28 | `DA4:47`; `CP:39` |
| Areas the records say it touches | Traefik slice with D5's errors middleware on `/auth/*` and a test that the limit's period is 60 s or less (`DA4:38, 147`). Rollouts slice. The AppProject limiting `platform/` and `environments/` (`CP:84-85`). The consumer test for Released versions v2.1 ("accepts a first file … before part 12 runs", `DA4:39`). `checks/promotion.py` and the values check (`GITOPS/AGENTS.md:46-47`). The promotion workflow in `.github/workflows/` (`GITOPS/AGENTS.md:49`). `environments/staging/`, `environments/production/` (`GITOPS/AGENTS.md:42`). The local CA and pull secrets, sealed by part 4's slices that need them (`GITOPS/AGENTS.md:38-40`; `CP:60-61`). `scripts/measure-client-address.sh` and `scripts/create-tenant.sh` (`GITOPS/AGENTS.md:44-45`). workloads.py for the chart's workloads (`RR2:311`). CI gitleaks (`RR2:279`) |  |
| Contracts | Consumer of Released versions v2.1 (`CP:15-16`). Reader of Release ledger v2 through checks/promotion (`CP:17-18`; `DA4:47`) |  |
| Shared fact | The web path `/errors/rate-limited`, which part 7 builds; "the path changes only through aidlc-design" | `CP:43-44`; `DA4:38` |
| Unknowns it answers | U1 (client address, Traefik slice), U2 (TraefikService routing, Rollouts slice), U3 (middleware on an HTTPRoute through ExtensionRef) | `CP:95, 98`; `ROOT/stories/NEW-jobwright/design-note.md:302` |
| Records say before part 4 | The owner chooses how to fit memory (raise WSL's limit, or fewer sessions) "before part 4's platform slices" (`GITOPS/plans/NEW-jobwright/part-2/plan.md:315-318`; no decision found: `grep -rn "R-n\|wslconfig"` over the records). A rehearsed restore of the sealing key on a throwaway cluster "before part 4" (`RR2:287-288`) |  |
| Index path note | Part 4 is the second part in jobwright-gitops, so `plans/NEW-jobwright/part-4/` matches 8.3.0's second-part rule (`AIDLC/skills/aidlc-build/SKILL.md:37`). The first part (part 2) is in `part-2/`, not in `plans/NEW-jobwright/` | `CP:46-48`; `git ls-tree` of `GITOPS` |
| Plan yet | No |  |

### NS-5 · part 5 · isolation in the database (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 3` | `CP:24` |
| Proves | AC4 (keys), AC5, AC25, NFR-DATA-02 | `CP:34`; `DA4:48` |
| Areas | schema-roles (RowLevelSecurityTests verbatim, schema check), tenant-context (ADR 0003 interceptors), migration-gates (up-down-up job; AC25's job; expand-only rule) | `P3/plan.md:117-120` |
| From design amendment 4 | AC25's job runs in the `integration` job from part 5 on; part 5 builds the `JOBWRIGHT_TEST_DATABASE` fixture switch | `DA4:36`; `DA4:144` |
| Contract | None changes, per the records. Expand-only migrations (`CP:24, 110`) |  |
| Plan yet | No (slice map only, `P3/plan.md:117-120`) |  |

### NS-6 · part 6 · API authorization and the v1 operations (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 5` | `CP:25` |
| Proves | AC4, AC6 (API), AC13, E2, E5, E20 | `CP:35`; `DA4:49` |
| Areas | api-auth (JwtBearer, membership and role per request, `X-Jobwright-Tenant`), api-v1 (`/v1/me`, tenant/settings, members, invitations), contract-tests (ContractTests.cs verbatim, ApiFixture, the drift test) | `P3/plan.md:122-124` |
| From design amendment 4 | Microsoft.OpenApi.YamlReader 2.12.2 in the contract test project only (D2, "Needed by part 6"); a check that no `api/src` project references it | `DA4:34, 145` |
| Contract | Implements Jobwright API v1 and closes its drift test. The contract changes only through aidlc-design (K7) | `P3/plan.md:65-67, 84-88` |
| Exception it ends | X-1 expires at "P3's contract-tests merge" (now part 6) or 2026-12-31, whichever is first | `P3/plan.md:96-102` |
| Plan yet | No |  |

### NS-7 · part 7 · sign-in and sessions (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 6` | `CP:26` |
| Proves | AC1 to AC3, AC14, AC16 (app share; the 429 route, D5), E1, E3, E4, E6 to E9, E12, E18, E25 | `CP:35`; `DA4:50` |
| Areas | web-session (`web_sessions`, `__Host-jw-session`), web-oidc (openid-client PKCE), web-api-client (openapi-fetch, the consumer test verbatim), browser-sign-in (test Keycloak realm) | `P3/plan.md:126-129` |
| Shared fact | Builds `GET /errors/rate-limited` (outside the session middleware, 429 problem, `Retry-After: 60`) that part 4's Traefik names | `CP:43-44`; `DA4:38` |
| Contract | Consumer side of Jobwright API v1 (consumer test) | `P3/plan.md:84-86, 128` |
| Plan yet | No |  |

### NS-8 · part 8 · the shell and accessibility (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 7` | `CP:27` |
| Proves | AC6 (offered), AC7 to AC12, AC26 on the Overview, E10, E11, E19 | `CP:36`; `DA4:51` |
| Areas | tokens-theme (provisional T1 delegate), navigation (T1 delegate), top-bar (T2; the tenant switch touches the session), accessibility (T2) | `P3/plan.md:131-135` |
| Owed findings from part 1 | M1 (dark really applied) and M2 (pages found, not listed) | `P3/plan.md:131, 134`; part 1 dispositions, PR #1 comment 6794032525 (`gh api`, read-only) |
| Plan yet | No |  |

### NS-9 · part 9 · telemetry (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 3` | `CP:28` |
| Proves | AC18 (telemetry), AC19, E16, E26 | `CP:36`; `DA4:52` |
| Areas | telemetry for api, worker and web (ADR 0009; an in-process exporter) | `P3/plan.md:137` |
| Plan yet | No |  |

### NS-10 · part 10 · the tenant command (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 5` | `CP:29` |
| Proves | AC29, E27 | `CP:37`; `DA4:53` |
| Areas | Jobwright.Admin against Keycloak and PostgreSQL; Mailpit as a test container pinned by digest (D3) | `P3/plan.md:139`; `DA4:35` |
| Plan yet | No |  |

### NS-11 · part 11 · gates, images, chart (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 3` | `CP:30` |
| Proves | AC17 (probes), AC22, AC24, AC27 (gates drill), E23, E28, E33 | `CP:37`; `DA4:54` |
| Areas | scans (gitleaks, trivy, syft; install-hooks.sh), chart (`charts/jobwright`), cluster-script-lint (provisional T2 delegate with checkpoints), gates-drill | `P3/plan.md:141-145` |
| Exception it ends | E-1 (the repeated-slash 308) expires "when P8 lands" (now part 11) | `P3/progress-web-headers.md:233-237` |
| Note from health-version | The readiness probe's `timeoutSeconds` must exceed 2 (C2); the worker gets a port with no route (C6) | `P3/progress-health-version.md:155-157, 171-172` |
| Plan yet | No |  |

### NS-12 · part 12 · the release job (STAGING_TOKEN) (jobwright-app)

| Fact | Value | Source |
|---|---|---|
| Waits for | `parts 3, 4, 5 to 11; see note` | `CP:31` |
| The note | "part 12 also waits for jobwright-gitops's NEW-slice0-review-fixes merged with its security gate given: a manual gate" (`CP:41`). "part 12 is not planned before part 4 and that gate" (`CP:42`). That gate is met: merged row done (`GITOPS/plans/NEW-slice0-review-fixes/README.md:29`), security gate given (`GITOPS/plans/NEW-jobwright/part-2/security-review.md:76`). The root index still says it is not (`ROOT/stories/NEW-jobwright/README.md:41`) |  |
| More waits in jobwright-gitops's records | M4 (the helm deny list) and L4 (job-level workflow permissions) "before part 3's release job holds STAGING_TOKEN" | `GITOPS/plans/NEW-slice0-review-fixes/release.md:30-31`; `RR2:195-196` |
| STAGING_TOKEN | Created again only "when part 12's plan is approved with part 4 merged and jobwright-gitops's review fixes merged with their security gate given" | `CP:74-76`; `DA4:40` |
| Proves | Released versions v2.1 (provider), E28, E31, E33 (release side) | `CP:37`; `DA4:55` |
| Contract | Provider of Released versions v2.1 (`CP:15-16`). Its first step shows `test_released_versions_v2_1_an_absent_file_is_created_as_run_zero` red (`DA4:148`) |  |
| Areas | publish_plan.py and staging_commit.py with their tests verbatim; the main-only job in environment `staging-release` | `P3/plan.md:147-148` |
| Order | "part 12 last" in jobwright-app (`CP:81`). "Part 12's first release before part 13" (`CP:83`) |  |
| Plan yet | No |  |

### NS-13 · part 13 · staging release, drill, AC16 end to end (jobwright-gitops)

| Fact | Value | Source |
|---|---|---|
| Waits for | `part 4; part 12 first release` | `CP:32` |
| Proves | AC20, AC21, AC16 end to end (U16) | `CP:39`; `DA4:56` |
| Areas | staging's Application, the release drill, the 11th sign-in attempt through Traefik | `DA4:56, 146, 157` |
| Contract | Deploys the images part 12 publishes (Released versions v2.1) | `CP:83` |
| Plan yet | No ("part 13's plan later", `DA4:197`) |  |

### NS-R · aidlc-release at the workspace root (NEW-jobwright)

| Fact | Value | Source |
|---|---|---|
| State | `to do` | `ROOT/stories/NEW-jobwright/README.md:44` |
| Waits for | Every part merged at the current versions (`--check-release`). From part 12 on, every merge releases to staging before the story is release-ready | `CP:87-89`; `ROOT/WORKSPACE.md:30-31` |
| Blockers | The root has no remote (`git -C ROOT remote` prints nothing; `SURVEY:56`), so 8.3.0's release check (`git fetch`, `origin/<default>`) has nothing to read (`AIDLC/skills/aidlc-release/SKILL.md:32`). No `criteria.md` in any part (`SURVEY:251`). No `api-v*` tag (`SURVEY:250`) |  |

### NS-G · jobwright-gitops later-story fixes from part 2's review (no story opened yet)

Each is "fix in a later story". None has an index or a plan. The only stated deadline is L4's.

| Finding | Disposition | Due | Source |
|---|---|---|---|
| L4 job-level `permissions:` unchecked (`checks/workflow_rules.py:37-52`) | fix in a later story, test first | before part 3's release job (now part 12) holds STAGING_TOKEN | `RR2:191-196`; `release.md:31` |
| M6 no push of several refs tested | fix in a later story | none stated | `RR2:126-132` |
| L2 symlinked KUBECONFIG | fix in a later story | none stated | `RR2:174-180` |
| L3 guard_rule.py blind spots (part) | fix in a later story | none stated | `RR2:182-189` |
| L5 / Q6 gitleaks and merge commits, `-c` vs `GITLEAKS_CONFIG` | fix in a later story, or record with part 4's CI gitleaks | none stated | `RR2:198-203, 317-319` |
| L7 install resume after an interruption | fix in a later story | none stated | `RR2:210-215` |
| L8 mode masks, failing `get`, lint.sh | fix in a later story | none stated | `RR2:217-233` |
| L10 `.gitignore` for `id_*`, `*.kubeconfig`, `.env*` | fix in a later story | none stated | `RR2:250-254` |
| Q4 workloads.py | fix in part 4, with the chart's workloads | part 4 | `RR2:307-311` |
| Q5 duplicate keys in yq vs client-go | fix in a later story, a test first | none stated | `RR2:312-316` |

Areas: `checks/`, `scripts/`, `.githooks/`, `.gitignore` in jobwright-gitops (from the cited lines).

### NS-A · jobwright-app findings from part 1's review "owed by part 3's plan"

The owner's dispositions on PR #1 (comment 6794032525, read with `gh api`): "L1, L2, L3, L5, L6, L7, L8, L9, L11, L12
and L15 are owed by part 3's plan. … Q1 goes to part 3's plan." Part 3's plan names only "part 1's review record (M1,
M2)" (`P3/plan.md:22`) and carries none of these IDs (`grep` over `P3/`: no hit). So they have no home yet
(INFERENCE: dropped, not dismissed). The findings are listed at `APP` `git show main:plans/NEW-jobwright/part-1/review-record.md`
(L1 to L19, Q1 to Q3). The same comment also owes:
- H2's exception, until "part 3's first module type" or 2027-01-31 (INFERENCE: now part 5);
- M3's exception (Admin's "Hello, World!", the Worker loop, web/README.md), until 2027-01-31 or the first slice that
  touches each file. The uncommitted health-version work deletes `api/src/Jobwright.Worker/Worker.cs` (`git status`);
- M4: ArchUnitNET's five transitive packages to be recorded in `new-system.md` v7 "before part 3". `new-system.md` has
  no `CycleDetection` (`grep`: no hit), so this is open.

---

## 3. Housekeeping and open findings

| # | Item | State and evidence | Who |
|---|---|---|---|
| HK1 | Root index row 18 "part 2 follow-up" | Reads `to do … plan v1 drafted` (`ROOT/stories/NEW-jobwright/README.md:41`). Stale: NEW-slice0-review-fixes merged (PR #4, `37639d2`) and the security gate is given (`GITOPS/…/NEW-slice0-review-fixes/README.md:28-29`). It is also a non-stage row, which 8.3.0's story-run stops on (`SURVEY:279-280`) | root session; owner approves |
| HK2 | Root boilerplate | "the person marks the merged row done" (5.0.0 wording) (`ROOT/stories/NEW-jobwright/README.md:17-18`); same line in `P3/README.md:14-15` | root and part sessions |
| HK3 | `status.md` stale | Part 3 reads `aidlc-plan`; parts 4 to 13 `waiting`; header "generated by scripts/story_status.py" (`ROOT/stories/NEW-jobwright/status.md:1, 7-17`). 8.3.0 removed the script (`SURVEY:199-202`). `WORKSPACE.md:29-31` still names it | root session |
| HK4 | Coordination plan Status column | `waiting` for all 13 parts (`CP:20-32`); line 45 derives status from the script | root session |
| HK5 | Part 3 index and plan not renamed | `LEVEL part 3 of 4` (`P3/README.md:9`); the plan's P2 to P9 not yet parts 5 to 12 (`P3/plan.md:2, 110-148`). OUTSIDE v5 asks for it (`CP:72-73`) | "Prashant Sharma or the next part 3 session" (`CP:72-73`) |
| HK6 | Part 3 index PINS and SOURCE | `coordination plan v4` and no home commit (`P3/README.md:6, 10`); bundles pin `(1.0.0-draft.1)` with no PLAYBOOK line (`SURVEY:148-150, 257`) | part 3 session |
| HK7 | Part 3 plan and bundles not on `main` | see IF-1 blockers (`AIDLC/skills/aidlc-build/SKILL.md:60`) | owner pushes a plan PR |
| HK8 | STAGING_TOKEN still exists | `gh api …/environments/staging-release/secrets` → `["STAGING_TOKEN"]`. D7: "Delete STAGING_TOKEN … now … and record in the story index that its secret list is empty" (`CP:74-76`; `DA4:40, 142`). The root index has no such record (`grep STAGING_TOKEN` hits only lines 41 and 43) | Prashant Sharma |
| HK9 | `integration` not a required check | the `gh api` result in IF-1b; due before part 3 merges (`CP:72`) | Prashant Sharma |
| HK10 | Unpushed branches | jobwright-app `feat/NEW-jobwright-part-3` (29 commits; IF-1); jobwright-gitops `chore/NEW-slice0-review-fixes-release` (1 commit; IF-2). Local-only older branches, all merged: app `feat/NEW-jobwright-onboard-prepare`, `chore/NEW-jobwright-amendment-1`; gitops the same two names (`git branch -a -vv`; `SURVEY:131, 183`) | Prashant Sharma |
| HK11 | Dirty trees | jobwright-app: 25 entries (IF-1a), including the owner's `.claude/settings.json` (K12). Root: `?? scripts/__pycache__/` (`git -C ROOT status --short`). story-run stops on a dirty tree (`SURVEY:264-265`) | owner; agents never stage `.claude/` |
| HK12 | `.worktrees/` not ignored | root, jobwright-app, jobwright-gitops (command in IF-1). lane-run Stop if (`AIDLC/skills/lane-run/SKILL.md:200`); `aidlc-onboard` adds the line (`:75-76`) | aidlc-onboard; owner approves |
| HK13 | No `QUEUE.md` anywhere, and the root has no remote | `QUEUE.md` counts only on the default branch on the remote, "in the home repository for a workspace" (`AIDLC/skills/parallel-lanes/SKILL.md:31-33`). The root has no remote (`SURVEY:56`) | owner (SURVEY H6, `:327-328`) |
| HK14 | M4 (part 2 review): helm deny list names verbs | Still four verbs (`GITOPS/.claude/settings.json:19-22`). Owner's commit; due before part 12 (`RR2:111-118`; `release.md:30`) | Prashant Sharma |
| HK15 | L11: check-ports.sh owner-only | AGENTS.md half done (`GITOPS/AGENTS.md:67-68`); no settings deny for it (`grep check-ports GITOPS/.claude/settings.json`: no hit). (`RR2:256-261`) | Prashant Sharma |
| HK16 | M9: kubectl 1.37.x not installed | `~/.local/opt/` has no kubectl-1.37.x (`ls`). Due "before part 3's first cluster run" (`RR2:154-156`; `CP:64-65`) | Prashant Sharma |
| HK17 | M10 and question 6 (Q2): the redis image, and gitleaks and yq on the machine line, into `new-system.md` v9 | Not there (`grep -i redis new-system.md`: no hit). (`RR2:163-164, 298-301`) | owner, at the root (aidlc-design) |
| HK18 | Part 1 M4: ArchUnitNET transitive packages in `new-system.md` | Not there (NS-A) | owner, at the root |
| HK19 | Question 4: rehearsed sealing-key restore before part 4 | No record of it (`grep`); `RR2:284-288` | Prashant Sharma |
| HK20 | Q1: re-run install-platform.sh with the default revision; give the root Application's revision | No record of it; `RR2:292-297` | Prashant Sharma |
| HK21 | Question 2: one approved `# gitleaks:allow` per sealed file until part 4's CI gitleaks | A standing rule, at each seal (`RR2:275-279`) | Prashant Sharma |
| HK22 | Part 2 plan R-n: memory before part 4's platform slices | No decision found (`GITOPS/plans/NEW-jobwright/part-2/plan.md:315-318`) | Prashant Sharma |
| HK23 | Feedback B | "B still open with aidlc" (`ROOT/feedback/2026-11-11-story-status-part-index/README.md:8-11`). INFERENCE: moot, since 8.3.0 removed the script (`SURVEY:199-201`) | owner closes it |
| HK24 | Exceptions with expiries | X-1: part 6's contract-tests merge or 2026-12-31 (`P3/plan.md:102`). E-1: when part 11 lands (`P3/progress-web-headers.md:233-237`). Part 1 H2 and M3: 2027-01-31 or the triggering slice (PR #1 comment) | Prashant Sharma |
| HK25 | Stale scripts and commands | `scripts/run_slices.py`, `scripts/check_recipe_commit.py` in both repositories, named in `APP/AGENTS.md:58-60` and `GITOPS/AGENTS.md:64-66`; `WORKSPACE.md:29-31` names `story_status.py` (`SURVEY:198-213`) | owner decides keep or delete |
| HK26 | Tracker not decided | "Tracker: not yet decided (requirements Q1)" (`ROOT/WORKSPACE.md:25-26`). Keys are provisional (`ROOT/stories/NEW-jobwright/README.md:5`) | Prashant Sharma (product owner) |
| HK27 | Every record is 5.0.0 | 85 "plugin version 5.0.0" lines; no BRANCH lines; no `criteria.md`; no decision-log rows (`SURVEY:44, 244-260`) | migration (SURVEY option a) |

---

## 4. Decisions only a person can make

The records name Prashant Sharma for every one (owner, tech lead, security owner and code owner: `CP:20-32, 116-118`).
Items D13 to D16 come from the survey, not from the story's records.

| # | Decision | Decider, as the records name them | Blocks | Source |
|---|---|---|---|---|
| D1 | Stop the k3d cluster, or say the integration and browser runs may run while it runs | Prashant Sharma (agents never run k3d) | IF-1a step 3 | `P3/progress-health-version.md:176-177, 205-207` |
| D2 | Add `integration` to the required checks on `main` (Q4) | Prashant Sharma | part 3's merge | `CP:72`; `P3/plan.md:258-259` |
| D3 | Approve the integration-gate bundle again with D4b's Testcontainers rule | the tech lead | NS-3b | `P3/bundle-integration-gate.md:5`; `DA4:196` |
| D4 | Confirm X-1, E-1, C4 and Option A in part 3's pull request | Prashant Sharma | part 3's review | `P3/plan.md:279-280`; `P3/progress-web-headers.md:226-237` |
| D5 | Review, approve and merge each part's pull request; push each branch | Prashant Sharma (KL2: GitHub records no approval for him as author) | every part | `CP:113-118`; `APP/AGENTS.md:119` |
| D6 | Approve each new part's plan (Tier 3) | Prashant Sharma (tech lead) | parts 4 to 13 | `CP:117`; `CP:49` |
| D7 | Delete STAGING_TOKEN now and record the empty list; create it again only when part 12's plan is approved with its preconditions | Prashant Sharma | part 12 | `CP:74-76`; `DA4:40, 142` |
| D8 | M4: deny every cluster helm verb in jobwright-gitops's settings (his commit) | Prashant Sharma | part 12 (as release.md says) | `RR2:116-118`; `release.md:30` |
| D9 | When to open the later story for L4 (and the other later-story fixes, NS-G) | Prashant Sharma (starts it with aidlc-start) | part 12 (L4 only) | `RR2:195-196`; `release.md:31` |
| D10 | Memory for part 4: raise WSL's limit or run fewer sessions | Prashant Sharma | part 4's platform slices | `GITOPS/plans/NEW-jobwright/part-2/plan.md:315-318` |
| D11 | Owner actions with evidence: rehearsed sealing-key restore (before part 4), install-platform.sh re-run (Q1), kubectl 1.37.x (M9), L11's settings deny, M10 and Q2 at the root | Prashant Sharma | part 4 (restore); otherwise none stated | `RR2:154-156, 163-164, 260-261, 287-288, 296-297, 301` |
| D12 | Where part 1's owed findings (L1–L3, L5–L9, L11, L12, L15, Q1) go, now that part 3's plan does not carry them | Prashant Sharma | none stated | PR #1 comment 6794032525; `P3/plan.md:22` |
| D13 | Whether to push the gitops release-record branch and merge it, or leave it local | Prashant Sharma | none | `SURVEY:322-323` (H4); IF-2 |
| D14 | A remote for the workspace root, so `QUEUE.md`, root document PRs and the release check work | Prashant Sharma | the coordinator itself; NS-R | `SURVEY:327-328` (H6); `AIDLC/skills/parallel-lanes/SKILL.md:31-33` |
| D15 | Part 3's plan pull request to `main` first ("plan merges first"), and how part 3's index and old notes migrate | Prashant Sharma | IF-1 under 8.3.0 | `SURVEY:346-351, 362-363` |
| D16 | The `.claude/settings.json` sandbox edit in jobwright-app: commit, revert or set aside (Q6 left it as is) | Prashant Sharma (an agent does not change a safety setting) | a clean tree for story-run and lanes | `P3/plan.md:89-90, 265-268`; `SURVEY:264-265` |
| D17 | The open unknowns: U1, U2, U3 (part 4), U16 (part 13) | Prashant Sharma (owner of each) | AC16, AC20, AC21 evidence | `CP:95-98`; `ROOT/stories/NEW-jobwright/design-note.md:302`; `DA4:157` |
| D18 | The tracker (requirements Q1) | Prashant Sharma (product owner) | none stated | `ROOT/WORKSPACE.md:25-26` |

---

## 5. Facts for the coordinator

### 5.1 The dependency graph as the records state it

| Item | Depends on | Source |
|---|---|---|
| IF-1 part 3 | parts 1, 2 (both merged) | `CP:22` |
| IF-1a health-version (resume) | web-headers (done); the owner's k3d word (D1) | `P3/README.md:44`; `P3/progress-health-version.md:9, 176-177` |
| NS-3b integration-gate | health-version done; bundle re-approved (D3) | `P3/README.md:45`; `P3/plan.md:199-201`; `DA4:196` |
| IF-1b part 3 verify, review, merge | integration-gate done; `integration` required (D2) | `P3/README.md:21-23, 45`; `P3/plan.md:281` |
| IF-2 gitops release record | none | `GITOPS/plans/NEW-slice0-review-fixes/README.md:20-29` |
| NS-4 part 4 | part 2 (merged); before its platform slices: D10; before part 4: the sealing-key restore (D11) | `CP:23`; `GITOPS/plans/NEW-jobwright/part-2/plan.md:315-318`; `RR2:287-288` |
| NS-5 part 5 | part 3 | `CP:24` |
| NS-6 part 6 | part 5 | `CP:25` |
| NS-7 part 7 | part 6 | `CP:26` |
| NS-8 part 8 | part 7 | `CP:27` |
| NS-9 part 9 | part 3 | `CP:28` |
| NS-10 part 10 | part 5 | `CP:29` |
| NS-11 part 11 | part 3 | `CP:30` |
| NS-12 part 12 | parts 3, 4, 5, 6, 7, 8, 9, 10, 11; review fixes merged with the security gate (met); not planned before part 4 and that gate; STAGING_TOKEN created only after its plan is approved; M4 and L4 first (jobwright-gitops records) | `CP:31, 41-42, 74-76, 84-87`; `release.md:30-31` |
| NS-13 part 13 | part 4; part 12's first release | `CP:32, 83` |
| NS-R root aidlc-release | every part merged (`--check-release`) | `CP:87-89`; `ROOT/stories/NEW-jobwright/README.md:44` |
| NS-G L4 later story | none stated; due before part 12 | `RR2:195-196` |
| NS-G other later-story fixes | none stated (Q4 goes into part 4) | `RR2:126-319` |
| HK8 STAGING_TOKEN delete | none ("now") | `CP:74` |

Order rules beyond Depends on, quoted:
- "(v5) In jobwright-app, one pull request at a time, each after aidlc-review (part 3 plan K10): part 3, then 5, 6, 7, 8
  in that order; 9 and 11 after part 3, and 10 after part 5, each placed between them by its readiness; part 12 last.
  Each part's aidlc-plan starts after the part it waits for has merged." (`CP:79-81`)
- "(v5) Part 4 (the platform, with the AppProject) may run alongside parts 3 and 5 to 11, and merges before part 12."
  (`CP:82`)
- "Part 12's first release before part 13: part 13 deploys the images part 12 publishes." (`CP:83`)
- "Contract first: api/contract/openapi.yaml lands in part 3's first slice, with its failing tests, before any API code."
  (`CP:77`) Done: the contract slice is done (`P3/progress-contract.md:9`).

The same graph in short (from the table above):

```text
jobwright-app:    3 --> 5 --> 6 --> 7 --> 8
                  |     '--> 10
                  |--> 9
                  '--> 11
                  (3, 5..11) --> 12
jobwright-gitops: 4 (needs only part 2, merged) --> 12 ; 4 + 12's first release --> 13
root:             all parts --> aidlc-release
no dependency:    IF-2 release record; NS-G later stories (L4 due before 12)
```

### 5.2 What shares a repository, a contract or files

Repository (stated: `CP:20-32`):

| Repository | Pending items |
|---|---|
| jobwright-app | IF-1 (with IF-1a, NS-3b, IF-1b), NS-5 to NS-12, NS-A |
| jobwright-gitops | IF-2, NS-4, NS-13, NS-G |
| workspace root | NS-R, HK1 to HK4, HK17, HK18, D14 |

Contracts and shared facts (stated unless marked):

| Contract or fact | Items | Source |
|---|---|---|
| Released versions v2.1 | provider part 12; consumer part 4 (its values check accepts a first file); part 13 deploys from it | `CP:15-16, 83`; `DA4:39` |
| Release ledger v2 | read by checks/promotion, part 4 (AC23) | `CP:17-18`; `DA4:47` |
| Jobwright API v1 (`api/contract/openapi.yaml`) | part 3 landed it; part 6 implements it and closes the drift test; part 7 holds the consumer test. The records plan no change to it: "Until then a change goes through aidlc-design" | `P3/plan.md:65-67, 84-88, 121-128` |
| The path `/errors/rate-limited` (a shared fact) | part 4 names it in Traefik; part 7 builds it; "the path changes only through aidlc-design" | `CP:43-44`; `DA4:38` |
| STAGING_TOKEN / `staging-release` | part 12 only (D7) | `CP:74-76` |
| `new-system.md` at the root | HK17, HK18; any further aidlc-design amendment | `RR2:163-164`; PR #1 comment |

Files and areas (MAY CHANGE where a bundle exists; otherwise the plan's slice map, and INFERENCE where marked):

| Path or area | Items | Source |
|---|---|---|
| `api/src/Jobwright.Api/Program.cs` | IF-1a (MAY CHANGE). INFERENCE: parts 5 (tenant-context interceptors), 6 (JwtBearer), 9 (telemetry) | `P3/bundle-health-version.md:38`; `P3/plan.md:119, 122, 137` |
| `api/src/Jobwright.Worker/**` | IF-1a. INFERENCE: part 9 (telemetry for the worker) | `P3/bundle-health-version.md:39`; `P3/plan.md:137` |
| `api/tests/Jobwright.IntegrationTests/**` | IF-1a creates it. INFERENCE: part 5's AC25 fixture switch | `P3/bundle-health-version.md:40`; `DA4:36` |
| `web/next.config.ts`, `web/tests/e2e/headers.spec.ts` | IF-1a (MAY CHANGE); also web-headers (done) | `P3/bundle-health-version.md:42`; `P3/bundle-web-headers.md:35-36` |
| `scripts/check.sh`, `scripts/tests/**` | NS-3b (MAY CHANGE); contract slice (done) | `P3/bundle-integration-gate.md:35`; `P3/bundle-contract.md:43-44` |
| `.github/workflows/ci.yml` | NS-3b (MAY CHANGE). INFERENCE: part 5 (the up-down-up and AC25 jobs), part 6 (contract steps), part 11 (scans, gates drill), part 12 (the release job) | `P3/bundle-integration-gate.md:35`; `P3/plan.md:120, 124, 141-148`; `DA4:36-37` |
| `Dockerfile.*` | NS-3b (MAY CHANGE). INFERENCE: part 11 (images, scans) | `P3/bundle-integration-gate.md:35-36`; `P3/plan.md:140-143` |
| web session and `web/src/proxy.ts` area | INFERENCE: part 7 (web-session, web-oidc, the 429 route outside the session middleware), part 8 (top-bar touches the session) | `P3/plan.md:126-129, 133`; `DA4:38` |
| `charts/jobwright/` | part 11 (chart) | `P3/plan.md:142-143` |
| `plans/NEW-jobwright/part-3/**` | every part 3 slice | each bundle's MAY CHANGE |
| jobwright-gitops `checks/`, `.github/workflows/` | part 4 (promotion.py, values check, promotion workflow). INFERENCE: the L4 later story (`checks/workflow_rules.py`) | `GITOPS/AGENTS.md:46-49`; `RR2:191` |
| jobwright-gitops `platform/`, `environments/` | part 4 (AppProject, values). Part 13 (staging's Application) | `CP:84`; `GITOPS/AGENTS.md:41-42`; `DA4:56` |
| jobwright-gitops `scripts/` | part 4 (`measure-client-address.sh`, `create-tenant.sh`). INFERENCE: NS-G (L2, L3, L7, L8 touch the guard and scripts) | `GITOPS/AGENTS.md:43-45`; `RR2:174-233` |
| jobwright-gitops `plans/NEW-slice0-review-fixes/` | IF-2 only | `git show --stat HEAD` |

### 5.3 Facts that decide how a lane may start (8.3.0), per item

| Item | Next stage | Tier / MODE | Plan approved by a person, and on `main`? | Can a sub-agent lane run it under 8.3.0? (reading of the rules only) |
|---|---|---|---|---|
| IF-1a health-version | build (resume step 3) | T2 / delegate with checkpoints | approved 2026-11-13 (`P3/plan.md:274-281`); **not on `main`** | The rules allow build for a delegate mode with an approved plan (`AIDLC/skills/parallel-lanes/SKILL.md:43-50`), but its wait needs the owner's word (D1) and build stops while the bundle is off `main` (`AIDLC/skills/aidlc-build/SKILL.md:60`) |
| NS-3b integration-gate | build | T3 / hands-on | bundle v1 approved but needs re-approval (D3); not on `main` | No: "a hands-on build … at Tier 3 waits for the person to confirm its restatement, so a person starts it" (`AIDLC/skills/parallel-lanes/SKILL.md:50-53`) |
| IF-1b part 3 verify, review | verify, then review | part T3 | as above | verify and review may go to a sub-agent on an approved plan (`:43-46`); review's reviewer passes come back to the coordinator (`:141-155`) |
| IF-2 release record | push and PR | n/a | n/a | No lane stage: a person pushes |
| NS-4 to NS-13 | intent/plan (no index) | provisional | no plan | No: "a person routes it with aidlc-start first" (`AIDLC/skills/lane-run/SKILL.md:53`); plan is started by a person (`AIDLC/skills/parallel-lanes/SKILL.md:52-53`) |
| NS-R | release | T3 | n/a | No: started by a person |

Cross-cutting blockers for any lane (all stated above): `.worktrees/` not ignored in all three (HK12); no `QUEUE.md`
and no root remote (HK13); part 3's branch checked out in the main copy with uncommitted work (IF-1); the part-path
gap (IF-1); agents may not push (IF-1).

UNKNOWN, could not confirm from files:
- Whether the k3d cluster is running now (no `docker ps` was run; `SURVEY:455-456`).
- Whether the owner has done any OUTSIDE action that leaves no file record (kubectl install was checked: not done).
- What "before part 3" means in the security gate (`GITOPS/plans/NEW-jobwright/part-2/security-review.md:76`). Release.md
  says "before part 3's release job" (`:30`). INFERENCE: part 12 under v5.
- Whether design amendment 4's integration-gate change was meant as a bundle revision or left to the build.
