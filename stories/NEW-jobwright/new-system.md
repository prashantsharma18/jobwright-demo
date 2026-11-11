<!-- aidlc template: new-system -->
# NEW-jobwright · Jobwright: the new system and its foundations · new system

## For the reviewer
- **Asked of:** The tech lead or architect, to approve the stack, every dependency and the scaffold commands
- **Record it:** On the APPROVED line below, and on the work item with this file's version and commit
- **Tier and main risk:** 3: a dependency or platform piece that does not behave as the design assumes (U1, U2, U7), or memory over 6 GB (U9)
- **Changed since the last version:** v7 (2026-11-11, design amendment 2, spec v4, ADR 0014): the main-branch run reads STAGING_TOKEN only in the job that names the environment `staging-release`; on `jobwright-app` the pull-request jobs are required checks on `main`. No dependency, contract or command changes. Before that, v6 (2026-11-11, from jobwright-app's part 1 plan v4, question Q14, approved at commit 6936a41 on branch `feat/NEW-jobwright-slice-0`): COMMANDS gives dev.sh's containers as 127.0.0.1:15432 (PostgreSQL) and 127.0.0.1:18080 (Keycloak) in place of 55432 and 58080, and says dev.sh runs the API on 127.0.0.1:15080 and the web app on 127.0.0.1:15300 (plan K4, decided by Prashant Sharma, 2026-11-11, E1); a note that on WSL2 with Docker Desktop, Windows' dynamic port exclusions (49152-65535 by default) can hold a host port, so the dev ports stay below that range. No dependency, contract, boundary or CI gate changes. Before that, v5 (2026-11-11, from jobwright-app's part 1 plan v3, question Q12, approved at commit 0991a3d on branch `feat/NEW-jobwright-slice-0`): xunit.v3.mtp-v2 4.0.2 replaces xunit.v3 in DEPENDENCIES and Pinned versions (Prashant Sharma, 2026-11-11, after the plan's v3 challenge W1 and W2: xunit.v3 is an alias whose platform package changes with its major version, and the xunit3 template never writes it); xunit.v3.templates 4.0.2 stays; SCAFFOLD's note says slice-0-skeleton changes the test projects to net10.0 only, and later test projects are generated with `dotnet new xunit3 --framework net10.0`; the v4 review lists' two test-project rows and the "xunit.v3 or xunit.v3.mtp-v2" alternative are corrected (mtp-v2 is in the graph under either choice); **v5 review lists** below list the transitive packages of plan Q11 and record the tool telemetry opt-outs of plan Q13, which COMMANDS also states. Before that, v4 (2026-11-10, from jobwright-app's part 1 plan v2, question Q10, and the recipe review R1 to R5 of commit 06a65b0): cn replaces clsx and tailwind-merge; @tailwindcss/turbopack replaces @tailwindcss/postcss; xunit.v3 alone, on Microsoft Testing Platform v2, replaces xunit.v3 with xunit.runner.visualstudio and Microsoft.NET.Test.Sdk (Prashant Sharma chose xunit.v3 over xunit.v3.mtp-v2, 2026-11-10); shadcn becomes a build-time devDependency; the scaffold writes `dotnet-tools.json` and `global.json` at the repository root (option A); **v4 review lists** below set every lock-file entry and PackageReference against DEPENDENCIES. Before that, v3 (2026-11-10, from jobwright-app's part 1 plan v1, questions Q1, Q2, Q6, Q7): the scaffold runs without prompts and writes no AGENTS.md (`--no-agents-md`, shadcn's flags, no dependency build script during the scaffold); two packages the generators add are approved; the .NET test filter is `--filter-trait`. Before that, v2 (2026-11-09, design amendment 1, spec v3): GitHub instead of Gitea, its runner, Docker-in-Docker and the local registry (ADR 0011, 0012, 0013); Released versions v2 and Release ledger v2; the CI lists for GitHub Actions; GitHub Actions pinned; Renovate off (R7)

Approving this file is the owner's agreement to add every dependency it names (requirements 9.5, spec rule). Versions
are in **Pinned versions** at the end; each is verified against its vendor's official page as of 2026-11-08, or marked
UNKNOWN there. The lock files that slice 0 writes pin every transitive package; a version a lock file resolves that is
not in this table is reviewed in slice 0's pull request.

```text
NEW SYSTEM · NEW-jobwright · Jobwright: the new system and its foundations
LAYOUT       several: jobwright-app, jobwright-gitops — the coordination plan holds one slice-0 part each (parts 1, 2)
CONTRACTS    Jobwright API v1: api -> web (jobwright-app; ADR 0004) · Released versions v2: jobwright-app CI -> jobwright-gitops (ADR 0012) · Release ledger v2 (rollout records): Prashant Sharma -> checks/promotion (ADR 0012)
NFR CHECKS   (this increment; S11)
             NFR-SEC-01: AC14 browser test · NFR-SEC-02/03: ProblemFormatContractTests (AC13) · NFR-SEC-04: AC16 test from two pods
             NFR-SEC-05: AC15 tests on every route class · NFR-SEC-06: gitleaks in CI (AC24) · NFR-SEC-07: trivy and syft in CI (AC24)
             NFR-SEC-08: chart test (non-root, read-only root file system) (AC24) · NFR-REL-01: AC20 in the staging drill
             NFR-REL-02: AC21 staging drill · NFR-REL-04: AC17 integration test · NFR-OPS-01: AC19 staging check
             NFR-OPS-04: AC18 test · NFR-ACC-01/02/04/05: AC12 axe scan and keyboard test, light and dark, 360 and 1440 px
             NFR-DATA-01: AC25 job and the up-down-up job · NFR-DATA-02: unit tests of time handling; a schema check that every timestamp is timestamptz
             NFR-PERF-03: AC26 budget test · FR-IAM-02: RowLevelSecurityTests and the schema check (AC4, AC5)
             (increment 4, S11: NFR-REL-03 production drill; NFR-PERF-01/04 load smoke; NFR-PERF-02 page speed with demo data;
             NFR-ACC-03 touch targets; NFR-OPS-02/03 dashboards, alerts, runbooks; NFR-DATA-03 retention)
QUALITY BAR  (no baseline exists yet; proposed for the team to set, measured first at the end of part 3)
             accessibility: no WCAG 2.2 AA violation on any page, light and dark (enforced, AC12)
             size: the Overview's first-load JavaScript at most the budget the plan sets from the skeleton (enforced, AC26)
             coverage: measured at the end of part 3; then no lower than that (ratchet; Measured until a check exists)
             latency: NFR-PERF-01 targets, measured in increment 4 (Measured)
UI           wireframes/app-shell.md v1, wireframes/access-pages.md v1 (drafts; product owner approves)

REPOSITORY   jobwright-app
STACK        Node.js LTS; Next.js 16 with React 19, TypeScript strict; Tailwind CSS 4; shadcn/ui — ADR: adr-draft-10.md, adr-draft-2.md
             .NET 10 (ASP.NET Core 10 API and worker, EF Core 10 with Npgsql) — ADR: adr-draft-1.md, adr-draft-3.md, adr-draft-4.md
             PostgreSQL 17 (row-level security) — ADR: adr-draft-3.md · OpenTelemetry — ADR: adr-draft-9.md
DEPENDENCIES web runtime: next, react, react-dom (the framework, 9.1); openid-client (OIDC with PKCE on the server, ADR 0002);
               openapi-fetch (calls the API through the generated types, ADR 0004); pg (the web role's access to web_sessions, ADR 0002);
               @opentelemetry/sdk-node, @opentelemetry/exporter-trace-otlp-http, @opentelemetry/exporter-metrics-otlp-http,
               @opentelemetry/exporter-logs-otlp-http, @opentelemetry/instrumentation-http (telemetry, ADR 0009);
               shadcn/ui's runtime set: radix-ui, class-variance-authority, cn (v4, in place of clsx and tailwind-merge), lucide-react, tw-animate-css (9.1)
             web development: typescript, @types/node, @types/react, @types/react-dom (v3, added by create-next-app), @types/pg, tailwindcss, @tailwindcss/turbopack (v4, in place of @tailwindcss/postcss) (9.1);
               shadcn (v4: a build-time devDependency, since globals.css imports shadcn/tailwind.css); openapi-typescript (generates the types); eslint, eslint-config-next;
               prettier (S5); vitest (unit and consumer tests); @playwright/test (browser tests, AC26 budget);
               @axe-core/playwright (AC12); pnpm (the package manager, pinned by packageManager)
             api: Microsoft.Extensions.Hosting (v3, the worker template's host); Npgsql.EntityFrameworkCore.PostgreSQL; Microsoft.EntityFrameworkCore.Design and the dotnet-ef tool (migration bundles);
               Microsoft.AspNetCore.OpenApi (the generated document for ContractDocumentTests); Microsoft.AspNetCore.Authentication.JwtBearer
               (token validation); OpenTelemetry.Extensions.Hosting, OpenTelemetry.Exporter.OpenTelemetryProtocol,
               OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.Http, Npgsql.OpenTelemetry (telemetry)
             api tests: xunit.v3.mtp-v2 (v5, in place of v4's xunit.v3: the name carries the platform's major version; on Microsoft Testing Platform v2;
               no xunit.runner.visualstudio or Microsoft.NET.Test.Sdk; its transitive packages in the v5 review lists), Microsoft.AspNetCore.Mvc.Testing,
               Testcontainers.PostgreSql, ArchUnitNET.xUnitV3 (module boundaries, ADR 0001; see Pinned versions)
             tools in CI: oasdiff (ADR 0004), gitleaks (NFR-SEC-06), trivy (NFR-SEC-07), syft (bills of materials),
               actionlint (workflow lint and the SHA rule), helm (chart rendering tests)
             images: the official node and mcr.microsoft.com/dotnet aspnet, runtime and sdk images, by digest; the
               Playwright image matching @playwright/test
PATHS        web/ (src/app, src/components/ui, src/server/{auth,api,session}, tests/e2e) · api/ (Jobwright.slnx, contract/openapi.yaml,
             src/Jobwright.Api, src/Jobwright.Worker, src/Jobwright.Admin, src/Jobwright.SharedKernel, src/Modules/Tenancy,
             src/Modules/Tenancy.Contracts, src/Modules/Identity, src/Modules/Identity.Contracts, tests/*) · charts/jobwright/ ·
             docs/adr/ · scripts/ · .github/workflows/ · Dockerfile.web, Dockerfile.api, Dockerfile.worker ·
             dotnet-tools.json and global.json at the repository root (v4, option A)
COMMANDS     tests: scripts/check.sh test · lint: scripts/check.sh lint · build: scripts/check.sh build · run: scripts/dev.sh   (onboard writes them to AGENTS.md as "not yet run")
             (check.sh test = pnpm -C web test && dotnet test api/Jobwright.slnx; lint = pnpm -C web lint (the ESLint CLI) && pnpm -C web format:check &&
             pnpm -C web typecheck && dotnet format api/Jobwright.slnx --verify-no-changes; build = pnpm -C web build &&
             dotnet build api/Jobwright.slnx -c Release && the three docker builds; dev.sh starts PostgreSQL 17 and Keycloak 26
             containers on 127.0.0.1:15432 and 127.0.0.1:18080, then runs the API on 127.0.0.1:15080 and the web app on
             127.0.0.1:15300, and refuses to start while the k3d cluster runs, to stay within memory)
             (v6, plan Q14: on WSL2 with Docker Desktop, Windows' dynamic port exclusions can hold a host port, and Docker
             Desktop then refuses to publish it. Windows' dynamic range is 49152-65535 by default, so the dev ports stay
             below that range. v5's 55432 and 58080 were inside it; on the owner's machine exclusions held 55432, 55080
             and 55300 (plan v4, E1). None of the four is a reserved lab port (requirements 9.5))
             (v5, plan Q13: tool telemetry off wherever the repository runs tools. check.sh and dev.sh export
             DOTNET_CLI_TELEMETRY_OPTOUT=1, TESTINGPLATFORM_TELEMETRY_OPTOUT=1 and NEXT_TELEMETRY_DISABLED=1; ci.yml sets
             them in a top-level env; each Dockerfile's build stage sets its own. See the v5 review lists)
SCAFFOLD     create-next-app 16.4.5 — `PNPM_CONFIG_STRICT_DEP_BUILDS=false pnpm create next-app@16.4.5 web --ts --tailwind --eslint --app --src-dir --import-alias "@/*" --use-pnpm --no-react-compiler --no-cache-components --no-agents-md --no-agent-feedback --yes` — web/
               (flags from the create-next-app reference; Cache Components off because a nonce-based CSP needs dynamic rendering, ADR 0010)
             shadcn — `PNPM_CONFIG_STRICT_DEP_BUILDS=false pnpm -C web dlx shadcn@4.23.0 init --defaults --base radix --yes --no-monorepo --no-rtl` then
               `add button card dropdown-menu sheet skeleton separator tooltip --yes` (same variable) — web/src/components/ui
             (v3: create-next-app writes AGENTS.md by default and shadcn add prompts unless `--yes` (nextjs.org; ui.shadcn.com/docs/cli);
               `--base radix` matches radix-ui below; the variable lets the scaffold's installs finish while no build script runs,
               since nothing is in allowBuilds (pnpm.io/settings/build); slice 0 then decides each build, with strict builds on)
             dotnet — `dotnet new sln -n Jobwright -o api` (slnx), `dotnet new webapi -n Jobwright.Api -o api/src/Jobwright.Api`,
               `dotnet new worker -n Jobwright.Worker -o api/src/Jobwright.Worker`, `dotnet new console -n Jobwright.Admin -o api/src/Jobwright.Admin`,
               `dotnet new classlib` for SharedKernel and each module and contracts project, `dotnet new install xunit.v3.templates::4.0.2`
               and `dotnet new xunit3` for each test project, `dotnet new tool-manifest && dotnet tool install dotnet-ef --version 10.0.14` — api/
             (slice 0 runs these and commits their output as an approved recipe commit; also `global.json` pinning SDK 10.0.403 with the test runner set to Microsoft Testing Platform)
             (v4, option A: on SDK 10.0.403 `dotnet new tool-manifest` writes `dotnet-tools.json` at the repository root, not
               `.config/`, and the xunit3 template's post-action writes the root `global.json` with the test runner only;
               slice-0-skeleton adds the SDK pin to it. The xunit3 template references xunit.v3.mtp-v2 and targets net8.0)
             (v5: slice-0-skeleton changes both test projects to net10.0 only; their xunit.v3.mtp-v2 reference stays.
               Later test projects are generated with `dotnet new xunit3 --framework net10.0`. The option is lowercase:
               the template's dotnetcli.host.json gives Framework the long name `framework`, and SDK 10.0.403 rejects
               `--Framework` as "not a valid option" (dry run, 2026-11-11))
CI           GitHub Actions on ubuntu-24.04 (ADR 0011); every gate reports on the pull request and blocks nothing on Free (KL1); in order (building-ci-pipelines), every one running a COMMANDS entry or a named tool:
             1 setup (pnpm install --frozen-lockfile with scripts blocked; dotnet restore --locked-mode) · 2 lint (check.sh lint, actionlint)
             3 unit (vitest; dotnet test --filter-trait "Category=Unit") · 4 contract (ContractDocumentTests, consumer tests, oasdiff from the second release)
             5 integration (dotnet test --filter-trait "Category=Integration" against Testcontainers PostgreSQL 17; RowLevelSecurityTests; schema check;
             up-down-up migrations; AC25 from the second release) · 6 scans (gitleaks; trivy fs on lock files)
             7 build once (three images; digests recorded; syft bill of materials; trivy image) · 8 browser (Playwright against the built
             images with a test Keycloak: AC1 to AC3, AC7 to AC12, AC14; axe) · 9 budget (AC26)
             on push to main only (concurrency group main-release): build, scan, bills of materials, browser tests against those digests,
             push to ghcr.io and move `main` (packages: write), then commit staging's versions to jobwright-gitops with STAGING_TOKEN,
             never backwards (ADR 0012; Released versions v2); STAGING_TOKEN is a secret of the environment staging-release (`main`
             only), and only that job names it (v7, ADR 0014, E31 v4)
             (v7) in jobwright-app every job that runs on pull requests is a required check on `main` by its job name, with no job-level
             `if:` or `paths` filter; adding or renaming one updates the protection rule in the same slice (ADR 0014, E28 v4)

REPOSITORY   jobwright-gitops
STACK        Kubernetes manifests, Helm values and Argo CD Applications (YAML); Bash for scripts; Python 3 standard library for checks — ADR: adr-draft-1.md, adr-draft-6.md, adr-draft-7.md, adr-draft-8.md
DEPENDENCIES platform (cluster): k3d with its k3s image (9.4); Traefik chart (9.4); Gateway API standard-channel CRDs (9.4); Argo CD (9.4);
               Argo Rollouts (9.4); Prometheus, prometheus-community "prometheus" chart, server only (9.4, ADR 0007); Sealed Secrets
               controller and kubeseal (9.4); PostgreSQL 17 official image (9.1); Keycloak 26 image (9.1); Mailpit (9.1);
               curlimages/curl (the canary's load job, ADR 0006)
             machine (outside the cluster): mkcert (the local certificate authority, ADR 0008); nothing else (ADR 0013)
             GitHub: GitHub Actions (hosted runners), GitHub Container Registry, an SSH deploy key (read only) for Argo CD (ADR 0011 to 0013)
             tools in CI: shellcheck (scripts), actionlint, kubeconform (manifest validation), helm (rendering), yq (reading values in checks)
             deferred (not approved by this design): MinIO or another object store, flagd and OpenFeature, a live-update backplane
PATHS        bootstrap/ (the owner's one-time setup) · platform/ (one Argo CD Application per component, pinned) ·
             environments/staging/ · environments/production/ · scripts/ (lib/guard.sh, create-tenant.sh, measure-memory.sh,
             measure-client-address.sh) · checks/ (promotion.py and its tests) · .github/workflows/
COMMANDS     tests: python3 -m unittest discover -s checks -p 'test_*.py' && scripts/test-guards.sh · lint: scripts/lint.sh ·
             build: scripts/render.sh (helm template and kubeconform for every Application) · run: none (Argo CD applies Git; the owner
             runs bootstrap/ once)   (onboard writes them to AGENTS.md as "not yet run")
SCAFFOLD     none: slice 0 writes the skeleton by hand, tests first (the guard's test first: it must refuse the default kubeconfig)
CI           required, in order: 1 lint (shellcheck, actionlint, the SHA rule, the guard-sourcing rule) · 2 tests (checks and guard tests)
             3 render (helm template and kubeconform for every Application and both environments) · 4 checks/promotion (on pull requests
             that change environments/production/; P4) · 5 values check (digests only, one revision, run number never lower)
             on every push to main, staging commits included: render, values check, and the production-change check (reports a change to
             environments/production/ that did not come from a merged promotion pull request; ADR 0012)
             on demand (workflow_dispatch): propose promotion (opens the production promotion pull request; ADR 0011)

APPROVED     Prashant Sharma, 2026-11-08, recorded on the work item with this file's version and commit (v1)
APPROVED v2  Prashant Sharma (tech lead), 2026-11-09, recorded at his instruction, with every dependency change above
APPROVED v3  Prashant Sharma (tech lead), 2026-11-10, recorded at his instruction: the scaffold flags (`--no-agents-md`, shadcn's
             flags, no dependency build script during the scaffold), the two generator-added packages (@types/react-dom within 19.3.x, a stop outside it;
             Microsoft.Extensions.Hosting 10.0.14, a stop if nuget.org has no such version) and `--filter-trait` for the .NET gates
APPROVED v4  Prashant Sharma (tech lead, design approver), 2026-11-10, approval delegated to Claude and recorded at
             Prashant Sharma's instruction: every change marked v4 above and in Pinned versions (cn 0.5.2,
             @tailwindcss/turbopack 4.3.5, xunit.v3 4.0.2 alone with no VSTest packages, shadcn as a build-time
             devDependency, the root dotnet-tools.json and global.json), against the v4 review lists below
APPROVED v5  Prashant Sharma (tech lead, design approver), 2026-11-11, recorded at Prashant Sharma's instruction: every
             change marked v5 above and in Pinned versions (xunit.v3.mtp-v2 4.0.2 as the test framework reference in
             place of xunit.v3, xunit.v3.templates 4.0.2 unchanged; the test projects to net10.0 only; later test
             projects by `dotnet new xunit3 --framework net10.0`; the corrected v4 review rows), against the v5 review
             lists below (plan Q11's transitive packages, plan Q13's tool telemetry opt-outs)
APPROVED v6  Prashant Sharma (tech lead, design approver), 2026-11-11, recorded at Prashant Sharma's instruction: every
             change marked v6 above (dev.sh's ports 127.0.0.1:15432, 18080, 15080 and 15300 in COMMANDS; the note on
             Windows' dynamic port exclusions)
APPROVED v7  ______ (Prashant Sharma, tech lead): the changes marked v7 above, with design amendment 2
```

## Pinned versions

Read on the vendors' official pages and registries on 2026-11-08 (checking-official-docs). FACT unless marked. The
rule used: the newest stable release, except where it is a new major or minor line only days old and an older line is
still supported (then the older line, so a fresh release does not land in the first build). Renovate is off until
GitHub Pro (R7): Prashant Sharma proposes later updates himself, as pull requests through the same gates.

| Component | Pin | Source | Note |
|---|---|---|---|
| Node.js | 26.13.0 (LTS "Lithium") | nodejs.org/dist/index.json | Active LTS since 2026-10-28, supported to 2029-04-30 |
| pnpm | 12.18.0 | registry.npmjs.org/pnpm | Pinned by `packageManager` |
| Next.js, eslint-config-next | 16.4.5 | registry.npmjs.org/next | `next lint` is removed in 16: lint is the ESLint CLI with flat config |
| React, React DOM | 19.3.1 | registry.npmjs.org/react | |
| TypeScript | 5.9.3 | registry.npmjs.org/typescript | Not 7.x: openapi-typescript's peer is `^5.x` |
| Tailwind CSS, @tailwindcss/turbopack | 4.3.5 | registry.npmjs.org | v4: @tailwindcss/turbopack replaces @tailwindcss/postcss (web/next.config.ts loads it; no PostCSS config exists). It ties Tailwind to Turbopack: `next build --webpack` and PostCSS-based tools get no Tailwind |
| shadcn (CLI) | 4.23.0 | registry.npmjs.org/shadcn | v4: a devDependency, needed at build time (globals.css imports `shadcn/tailwind.css`), not at run time; the image ships only the standalone output |
| Prettier | 3.9.10 | registry.npmjs.org/prettier | S5 |
| ESLint | 10.14.0 | registry.npmjs.org/eslint | Supported by eslint-config-next from 16.4.0 |
| Vitest | 5.1.0 | registry.npmjs.org/vitest | |
| @playwright/test | 1.65.0; image `mcr.microsoft.com/playwright:v1.65.0-noble` | registry.npmjs.org; mcr.microsoft.com | |
| @axe-core/playwright | 4.14.0 | registry.npmjs.org | |
| openapi-typescript | 7.13.0 | registry.npmjs.org | |
| openapi-fetch | 0.17.0 | registry.npmjs.org | Pre-1.0 |
| openid-client | 6.8.9 | registry.npmjs.org | Auth.js v5 is still beta; not used |
| OpenTelemetry JS | sdk-node, exporter-trace-otlp-http, exporter-metrics-otlp-http, exporter-logs-otlp-http, instrumentation-http 0.223.0; `@opentelemetry/api` 1.9.1 | registry.npmjs.org | Core 2.12.0. Not 0.301 (core 3.x, 3 days old). api 1.9.1, because sdk-node 0.223's peer is `>=1.3.0 <1.10.0`. `@vercel/otel` not used |
| .NET SDK, runtime | SDK 10.0.403, runtime 10.0.14 (LTS) | builds.dotnet.microsoft.com release metadata | Supported to 2028-11-14 |
| ASP.NET Core packages (OpenApi, JwtBearer, Mvc.Testing), EF Core, EF Design, dotnet-ef | 10.0.14 | nuget.org | OpenAPI 3.1 is .NET 10's default (learn.microsoft.com) |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | nuget.org | Requires EF Core 10.0.4 or later in 10.x |
| xunit.v3.mtp-v2, xunit.v3.templates | 4.0.2 | nuget.org; xunit.net/docs/getting-started/v3/microsoft-testing-platform (read 2026-11-10 by jobwright-app's plan v2); api.nuget.org nuspecs of xunit.v3 3.2.2 and 4.0.2 and xunit.v3.mtp-v2 4.0.2, and xunit.v3.templates 4.0.2's TestProject.csproj (read 2026-11-11 by jobwright-app's plan v3) | Microsoft Testing Platform v2, switched on for `dotnet test` in `global.json`; filter by trait with `--filter-trait`, VSTest's `--filter` is rejected (xunit.net, v3). v5: xunit.v3.mtp-v2 replaces v4's xunit.v3. It is what the xunit3 template writes, and its name carries the platform's major version, while xunit.v3 is an alias (3.2.2 depends on exactly xunit.v3.mtp-v1, 4.0.2 on exactly xunit.v3.mtp-v2). xunit.v3 is not referenced. Its own dependencies xunit.v3.core.mtp-v2 and xunit.v3.assert must resolve to exactly 4.0.2, or slice-0-skeleton stops (plan Q11) |
| xunit.runner.visualstudio, Microsoft.NET.Test.Sdk | not used (v4) | xunit.net (v3, Microsoft Testing Platform) | v4: removed; they serve VSTest mode only |
| Testcontainers.PostgreSql | 4.16.0 | nuget.org | |
| OpenTelemetry .NET | Extensions.Hosting 1.20.1, Exporter.OpenTelemetryProtocol 1.20.1, Instrumentation.AspNetCore 1.20.2, Instrumentation.Http 1.20.2; Npgsql.OpenTelemetry 10.0.3 | nuget.org | |
| Architecture tests | TngTech.ArchUnitNET.xUnitV3 0.13.5 | nuget.org; github.com/TNG/ArchUnitNET | Active, pre-1.0; NetArchTest is unmaintained (last release 2021). It restores against xunit.v3 4.x, but runtime support is UNKNOWN (U13): slice 0's smoke test proves it, and the fallback is a reflection test over project references with no library |
| Assertions | xUnit's own | — | FluentAssertions 8 needs a paid licence for commercial use; not used |
| PostgreSQL | 17.11, image `postgres:17.11` | postgresql.org; hub.docker.com/_/postgres | Official image, not Bitnami; supported to 2029-11-08 |
| Keycloak | 26.8.2, image `quay.io/keycloak/keycloak:26.8.2` | github.com/keycloak/keycloak/releases | Organizations are a supported feature |
| Mailpit | 1.33.1, image `axllent/mailpit:v1.33.1` | github.com/axllent/mailpit/releases | |
| k3d | 5.9.0, with `--image rancher/k3s:v1.37.2-k3s1` set explicitly; no local registry (ADR 0013) | github.com/k3d-io/k3d/releases; hub.docker.com/r/rancher/k3s | No documented compatibility limit for 1.37 (INFERENCE: current stable channel, which k3d tests); proved in slice 0 |
| Traefik | 3.7.17, chart 42.3.1 | github.com/traefik/traefik/releases; traefik.github.io/charts | Supports Gateway API v1.6.3; `crossProviderNamespaces` set explicitly (its documented default is contradictory) |
| Gateway API CRDs | v1.6.3, standard channel | doc.traefik.io (supported version) | Not 1.7.0, which Traefik does not yet support |
| Argo CD | 3.5.7, chart `argo-cd` 10.11.7 | github.com/argoproj/argo-cd/releases; argoproj.github.io/argo-helm | 3.6.x is days old; 3.5 is still patched |
| Argo Rollouts | 1.10.2, chart `argo-rollouts` 2.43.11 | github.com/argoproj/argo-rollouts/releases; argoproj.github.io/argo-helm | |
| Prometheus | 3.16.0, chart `prometheus` 29.45.1 | prometheus-community.github.io/helm-charts | Server only; not kube-prometheus-stack |
| Sealed Secrets | controller and kubeseal 0.41.2, chart 2.22.0 from bitnami-labs | bitnami.github.io/sealed-secrets | Not the frozen bitnami/charts copy (app 0.31.0) |
| Renovate | off until GitHub Pro (R7) | — | Its hosted app's free tier covers private repositories (docs.renovatebot.com); not installed now |
| Trivy | 0.76.2 | github.com/aquasecurity/trivy/releases | Never 0.69.4 to 0.69.6 (malicious builds, GHSA-69fq-xp46-6x23); pinned by digest |
| Syft | 1.57.0 | github.com/anchore/syft/releases | |
| gitleaks | 8.30.1 | github.com/gitleaks/gitleaks/releases | Slow release pace; used by CI and the pre-push hook (E23 v2) |
| oasdiff | 1.36.0 | github.com/oasdiff/oasdiff/releases | |
| GitHub Actions (pinned by commit SHA) | actions/checkout v7.0.1 `3d3c42e5aac5ba805825da76410c181273ba90b1` · actions/setup-node v7.1.0 `949feb2413d6458794dcd2491c4babbbce0c15c1` · pnpm/action-setup v6.1.1 `73e46992e05b19abd31f2f3c20e0e7370d762115` · actions/setup-dotnet v6.1.0 `0f4a354dd5547a28c70b7853dbd95d5b67886e73` · actions/cache v6.1.0 `55cc8345863c7cc4c66a329aec7e433d2d1c52a9` · docker/setup-buildx-action v4.4.1 `f87e5991a6d7451dcb8d9637bfbc97413f497069` · docker/login-action v4.8.0 `8aa5831fb8d48d8f76594c7ddde61ae58019f550` · docker/build-push-action v7.4.0 `c3c9e263c25d99ce0380d002d59b67737d91b0dc` · docker/metadata-action v6.2.0 `dc802804100637a589fabce1cb79ff13a1411302` · actions/upload-artifact v7.0.2 `cf430e030ddbb5b0abf93d22962f4752f3646cd9` · aquasecurity/trivy-action v0.36.0 `ed142fd0673e97e23eac54620cfb913e5ce36c25` (with `version: v0.76.2`) · anchore/sbom-action v0.24.3 `66cbf4bc1f1c0d2edc94016e65bc221b6bb0ad6c` · gitleaks/gitleaks-action v3.0.0 `e0c47f4f8be36e29cdc102c57e68cb5cbf0e8d1e` | api.github.com releases and git refs of each repository, cross-checked with `git ls-remote` | Annotated tags resolved to their commits. trivy-action 0.36.0 is an immutable release after the March 2026 compromise (GHSA-69fq-xp46-6x23). gitleaks-action is under its own licence; a personal account's repositories need no licence key. actions/delete-package-versions is not used (P2). actionlint 1.7.12 is a binary, not an action |
| GitHub-hosted runner | `ubuntu-24.04` | docs.github.com | A fixed label, never `ubuntu-latest` |
| create-next-app | 16.4.5 | registry.npmjs.org; nextjs.org (create-next-app reference) | Scaffolding only |
| pg, @types/pg | 8.23.1 | registry.npmjs.org | |
| shadcn's runtime set | radix-ui 1.8.1, class-variance-authority 0.7.1, cn 0.5.2, lucide-react 1.70.0, tw-animate-css 1.4.0 | registry.npmjs.org | v4: cn replaces clsx 2.1.1 and tailwind-merge 3.7.0 (see below) |
| cn | 0.5.2 | registry.npmjs.org/cn (read 2026-11-10 by jobwright-app's recipe review, R1) | v4. shadcn's own package (maintainer shadcn, repository github.com/shadcn-ui/cn, published by GitHub Actions with SLSA provenance v1). Pre-1.0; the name was reused (0.1.x from 2013, shadcn's from 0.2.0 on 2026-09-01). slice-0-skeleton's cn test pins its behaviour |
| @types/node, @types/react | 26.6.6, 19.3.0 | registry.npmjs.org | |
| @types/react-dom | 19.3.x, the patch create-next-app resolves (UNKNOWN until jobwright-app's slice-0-scaffold; its progress note records it) | registry.npmjs.org | v3; a version outside 19.3.x is a stop |
| Microsoft.Extensions.Hosting | 10.0.14 (INFERENCE: released with runtime 10.0.14; checked on nuget.org in slice-0-skeleton, a stop if absent) | nuget.org | v3; revisited when the worker gains health and version endpoints (spec A3) |
| mkcert | 1.4.4 | github.com/FiloSottile/mkcert/releases | No release since 2022; small and stable; run once by the owner |
| curlimages/curl | 8.23.0 | hub.docker.com/r/curlimages/curl | The canary load job |
| shellcheck, actionlint, kubeconform, yq | 0.11.0, 1.7.12, 0.8.0, 4.54.2 | their GitHub releases | |
| helm (CLI, for rendering tests) | 4.3.1 | github.com/helm/helm/releases | A new major; the charts above are rendered with it in slice 0's render check |
| Deferred, not approved | object storage (MinIO's community edition is archived, with no images since 2025-10; an alternative is chosen in the increment that needs it), flagd and OpenFeature | github.com/minio/minio | |

## v4 review lists

Written for the v4 approval (jobwright-app plan v2, Q10 and challenge V1), so it does not rest on a summary. Read from
jobwright-app's recipe commit 06a65b0 (branch `feat/NEW-jobwright-slice-0`) on 2026-11-10: `web/package.json`,
`web/pnpm-lock.yaml`, `web/pnpm-workspace.yaml` and every `*.csproj`. v5 corrects the two test-project rows and the
xunit alternative (plan v3, Q12); everything else stands as approved with v4.

**web/pnpm-lock.yaml, importer `.`**: every direct entry, against DEPENDENCIES.

| Entry | Specifier | Resolved | DEPENDENCIES | slice-0-skeleton pins |
|---|---|---|---|---|
| class-variance-authority | ^0.7.1 | 0.7.1 | listed | 0.7.1 |
| cn | ^0.5.2 | 0.5.2 | v4 (R1) | 0.5.2 |
| lucide-react | ^1.70.0 | 1.70.0 | listed | 1.70.0 |
| next | 16.4.5 | 16.4.5 | listed | 16.4.5 |
| radix-ui | ^1.8.1 | 1.8.1 | listed | 1.8.1 |
| react, react-dom | 19.3.0 | 19.3.0 | listed | 19.3.1 (R6) |
| shadcn | ^4.23.0, in dependencies | 4.23.0 | v4: devDependency (R5) | 4.23.0, moved to devDependencies |
| tw-animate-css | ^1.4.0 | 1.4.0 | listed | 1.4.0 |
| @tailwindcss/turbopack (dev) | ^4 | 4.3.5 | v4 (R2) | 4.3.5 |
| @types/node (dev) | ^20 | 20.19.43 | listed | 26.6.6 (R6) |
| @types/react (dev) | ^19 | 19.3.0 | listed | 19.3.0 |
| @types/react-dom (dev) | ^19 | 19.3.0 | listed (v3) | 19.3.0, within 19.3.x |
| eslint (dev) | ^9 | 9.39.5 | listed | 10.14.0 (R6) |
| eslint-config-next (dev) | 16.4.5 | 16.4.5 | listed | 16.4.5 |
| tailwindcss (dev) | ^4 | 4.3.5 | listed | 4.3.5 |
| typescript (dev) | ^5 | 5.9.3 | listed | 5.9.3 |
| pnpm (packageManagerDependencies) | 12.18.0 | 12.18.0 | listed | 12.18.0 |

- Not in the lock and no longer in DEPENDENCIES (v4): clsx and tailwind-merge as direct entries, @tailwindcss/postcss.
  clsx 2.1.1 stays in the lock as class-variance-authority's own dependency; tailwind-merge is not in the lock at all.
- In the lock but imported by no generated file: cn 0.2.6, the shadcn CLI's own dependency.
- In DEPENDENCIES but not yet in the lock (added later by slice 0 or its successors): openid-client, openapi-fetch, pg,
  @types/pg, the OpenTelemetry packages, openapi-typescript, prettier, vitest, @playwright/test, @axe-core/playwright.

**web/pnpm-workspace.yaml `allowBuilds`**, written by the scaffold: `sharp: false`, `unrs-resolver: false`. No
dependency build script runs. slice 0 decides each build with strict builds on (v3).

**PackageReferences and tools**, against DEPENDENCIES.

| Project or file | Reference | Version | DEPENDENCIES | slice-0-skeleton |
|---|---|---|---|---|
| Jobwright.Api | Microsoft.AspNetCore.OpenApi | 10.0.14 | listed | moved to the central pin |
| Jobwright.Worker | Microsoft.Extensions.Hosting | 10.0.14 | listed (v3) | moved to the central pin |
| Jobwright.Api.Tests | xunit.v3.mtp-v2 | 4.0.2 | listed (v5; v4 had chosen xunit.v3) | kept, moved to the central pin; net8.0 to net10.0 only (R4) |
| Jobwright.ArchitectureTests | xunit.v3.mtp-v2 | 4.0.2 | listed (v5; v4 had chosen xunit.v3) | kept, moved to the central pin; net8.0 to net10.0 only (R4) |
| Jobwright.Admin, Jobwright.SharedKernel, the four module and contracts projects | none | — | — | — |
| dotnet-tools.json (repository root) | dotnet-ef | 10.0.14 | listed | — |
| global.json (repository root) | `test.runner`: Microsoft.Testing.Platform | — | listed (SCAFFOLD) | adds the SDK pin 10.0.403 |

**Alternatives not taken, with their cost.**
- clsx and tailwind-merge in place of cn: every generated component imports `from "cn"` directly (7 files under
  web/src/components/ui, plus web/src/lib/utils.ts), so each import changes now and again after every `shadcn add`
  (challenge V2).
- shadcn/tailwind.css copied into globals.css in place of shadcn as a devDependency: shadcn's CSS updates no longer
  arrive with the package. It would keep the CLI and its dependencies (among them @modelcontextprotocol/server and
  @dotenvx/dotenvx) out of the install (challenge V3).
- @tailwindcss/postcss in place of @tailwindcss/turbopack: needs a PostCSS config and a change to the generated
  next.config.ts. Keeping turbopack ties Tailwind to Turbopack (challenge V9).
- xunit.v3 in place of xunit.v3.mtp-v2 (v4's choice; corrected in v5, plan Q12): it would not remove a package.
  xunit.v3 4.0.2 is an empty package whose only dependency is exactly xunit.v3.mtp-v2 [4.0.2], so mtp-v2 and its
  graph are in the lock files under either choice; the choice is only which name the projects reference. xunit.v3
  hides the platform's major version (3.2.2 resolves xunit.v3.mtp-v1, 4.0.2 resolves xunit.v3.mtp-v2), so a one-line
  bump could switch the platform unseen. The xunit3 template never writes it, so both test projects and every later
  `dotnet new xunit3` output would need a hand edit that nothing enforces (plan v3, W1 and W2).

**Follow-up in jobwright-app before slice-0-skeleton builds.** Plan v2's Q1 (R3) and the slice-0-skeleton bundle name
xunit.v3.mtp-v2. Choosing xunit.v3 changes both again before the build (plan Q10). That is aidlc-plan's work in
jobwright-app, not this file's.

## v5 review lists

Written for the v5 approval (jobwright-app plan v3, Q11 to Q13).

**Transitive packages of the test projects (plan Q11).** Prashant Sharma decided on 2026-11-11 that xunit's and the
platform's transitive packages are expected at slice-0-skeleton's step 2 CHECKPOINT. They are listed in the pull
request and are not a stop. A stop is xunit.v3.core.mtp-v2 or xunit.v3.assert at any version other than 4.0.2, or a
package in the xunit or platform graph that comes from outside Microsoft, xUnit or this list. Each row below was read
from the scaffold's `obj/project.assets.json` of both test projects (net8.0, untracked, read 2026-11-11), with the
package that brings it in. The ArchUnitNET rows were read from its nuspec, because slice-0-skeleton adds that
reference. The net10.0 lock files that slice-0-skeleton writes are new: restore prunes packages the framework already
provides, so they may hold fewer packages than this list (plan W6).

| Package | Version | Brought in by | Plan Q11 | Note |
|---|---|---|---|---|
| xunit.v3.core.mtp-v2 | 4.0.2 | xunit.v3.mtp-v2 [4.0.2] | named | Any other version is a stop. Holds the build logic (`buildTransitive/`) |
| xunit.v3.assert | 4.0.2 | xunit.v3.mtp-v2 [4.0.2]; TngTech.ArchUnitNET.xUnitV3 (>= 1.1.0) | named | Any other version is a stop |
| xunit.analyzers | 2.2.0 | xunit.v3.mtp-v2 | named (xunit's graph) | |
| xunit.v3.common, xunit.v3.extensibility.core, xunit.v3.runner.common, xunit.v3.runner.inproc.console | 4.0.2 | xunit.v3.core.mtp-v2 and each other, all exact [4.0.2] | xunit's graph | |
| Microsoft.Testing.Platform, Microsoft.Testing.Platform.MSBuild, Microsoft.Testing.Extensions.TrxReport.Abstractions | 2.5.1 | xunit.v3.core.mtp-v2 | the platform's graph | Microsoft Testing Platform v2 |
| Microsoft.Testing.Extensions.Telemetry | 2.5.1 | xunit.v3.core.mtp-v2 | named | Sends usage data to Microsoft by default; switched off by Q13 (below) |
| Microsoft.ApplicationInsights | 2.23.0 | Microsoft.Testing.Extensions.Telemetry | named | The telemetry extension's transport |
| Microsoft.Win32.Registry | 5.0.0 | xunit.v3.runner.common [5.0.0] | named | |
| System.Security.AccessControl | 6.0.1 | xunit.v3.runner.common [6.0.1]; Microsoft.Win32.Registry (5.0.0, raised to 6.0.1) | named | |
| Microsoft.Bcl.AsyncInterfaces | 6.0.0 | xunit.v3.common | named | |
| System.Diagnostics.DiagnosticSource | 5.0.0 | Microsoft.ApplicationInsights | not named by Q11 | Microsoft's own, in the platform's graph; found in the assets file for this list |
| System.Security.Principal.Windows | 5.0.0 | Microsoft.Win32.Registry | not named by Q11 | Microsoft's own, in the platform's graph; found in the assets file for this list |
| TngTech.ArchUnitNET | 0.13.5 | TngTech.ArchUnitNET.xUnitV3 0.13.5 | ArchUnitNET's graph | The library behind the approved TngTech.ArchUnitNET.xUnitV3 (Pinned versions) |
| System.Net.Http | 4.3.4 | TngTech.ArchUnitNET (nuspec) | named | |
| System.Text.RegularExpressions | 4.3.1 | TngTech.ArchUnitNET (nuspec) | named | |

**Tool telemetry off (plan Q13).** Prashant Sharma decided on 2026-11-11, from the plan's v3 challenge W3, that tool
telemetry is switched off wherever jobwright-app runs tools. It adds no dependency.

| Variable | Switches off | Source |
|---|---|---|
| `DOTNET_CLI_TELEMETRY_OPTOUT=1` | The .NET SDK's telemetry, and Microsoft Testing Platform's too | learn.microsoft.com/dotnet/core/tools/telemetry |
| `TESTINGPLATFORM_TELEMETRY_OPTOUT=1` | Microsoft Testing Platform's telemetry extension (Microsoft.Testing.Extensions.Telemetry) | learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-telemetry |
| `NEXT_TELEMETRY_DISABLED=1` | Next.js's telemetry | nextjs.org/telemetry |

Where they are set: `scripts/check.sh` exports all three before any command (slice-0-skeleton step 8); `scripts/dev.sh`
exports them too (slice-0-ci step 1); `.github/workflows/ci.yml` sets them in a top-level `env`, and each Dockerfile's
build stage sets its own (slice-0-ci steps 3 and 4). A person's own shell is theirs to set. Why: these tools send
usage data to their vendors by default, and the requirements send telemetry only to the owner's lab (requirements 9.1,
"Telemetry: OpenTelemetry to the owner's lab collector").
