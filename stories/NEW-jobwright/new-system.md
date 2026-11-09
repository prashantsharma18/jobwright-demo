<!-- aidlc template: new-system -->
# NEW-jobwright · Jobwright: the new system and its foundations · new system

## For the reviewer
- **Asked of:** The tech lead or architect, to approve the stack, every dependency and the scaffold commands
- **Record it:** On the APPROVED line below, and on the work item with this file's version and commit
- **Tier and main risk:** 3: a dependency or platform piece that does not behave as the design assumes (U1, U2, U7), or memory over 6 GB (U9)
- **Changed since the last version:** first version

Approving this file is the owner's agreement to add every dependency it names (requirements 9.5, spec rule). Versions
are in **Pinned versions** at the end; each is verified against its vendor's official page as of 2026-11-08, or marked
UNKNOWN there. The lock files that slice 0 writes pin every transitive package; a version a lock file resolves that is
not in this table is reviewed in slice 0's pull request.

```text
NEW SYSTEM · NEW-jobwright · Jobwright: the new system and its foundations
LAYOUT       several: jobwright-app, jobwright-gitops — the coordination plan holds one slice-0 part each (parts 1, 2)
CONTRACTS    Jobwright API v1: api -> web (jobwright-app; ADR 0004) · Released versions v1: jobwright-app CI -> jobwright-gitops (ADR 0006) · Release ledger v1: the cluster -> checks/promotion (ADR 0006)
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
               shadcn/ui's runtime set: radix-ui, class-variance-authority, clsx, tailwind-merge, lucide-react, tw-animate-css (9.1)
             web development: typescript, @types/node, @types/react, @types/pg, tailwindcss, @tailwindcss/postcss (9.1);
               shadcn (the CLI, scaffolding only); openapi-typescript (generates the types); eslint, eslint-config-next;
               prettier (S5); vitest (unit and consumer tests); @playwright/test (browser tests, AC26 budget);
               @axe-core/playwright (AC12); pnpm (the package manager, pinned by packageManager)
             api: Npgsql.EntityFrameworkCore.PostgreSQL; Microsoft.EntityFrameworkCore.Design and the dotnet-ef tool (migration bundles);
               Microsoft.AspNetCore.OpenApi (the generated document for ContractDocumentTests); Microsoft.AspNetCore.Authentication.JwtBearer
               (token validation); OpenTelemetry.Extensions.Hosting, OpenTelemetry.Exporter.OpenTelemetryProtocol,
               OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.Http, Npgsql.OpenTelemetry (telemetry)
             api tests: xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Microsoft.AspNetCore.Mvc.Testing,
               Testcontainers.PostgreSql, ArchUnitNET.xUnitV3 (module boundaries, ADR 0001; see Pinned versions)
             tools in CI: oasdiff (ADR 0004), gitleaks (NFR-SEC-06), trivy (NFR-SEC-07), syft (bills of materials),
               actionlint (workflow lint and the SHA rule), helm (chart rendering tests)
             images: the official node and mcr.microsoft.com/dotnet aspnet, runtime and sdk images, by digest; the
               Playwright image matching @playwright/test
PATHS        web/ (src/app, src/components/ui, src/server/{auth,api,session}, tests/e2e) · api/ (Jobwright.slnx, contract/openapi.yaml,
             src/Jobwright.Api, src/Jobwright.Worker, src/Jobwright.Admin, src/Jobwright.SharedKernel, src/Modules/Tenancy,
             src/Modules/Tenancy.Contracts, src/Modules/Identity, src/Modules/Identity.Contracts, tests/*) · charts/jobwright/ ·
             docs/adr/ · scripts/ · .github/workflows/ · Dockerfile.web, Dockerfile.api, Dockerfile.worker
COMMANDS     tests: scripts/check.sh test · lint: scripts/check.sh lint · build: scripts/check.sh build · run: scripts/dev.sh   (onboard writes them to AGENTS.md as "not yet run")
             (check.sh test = pnpm -C web test && dotnet test api/Jobwright.slnx; lint = pnpm -C web lint (the ESLint CLI) && pnpm -C web format:check &&
             pnpm -C web typecheck && dotnet format api/Jobwright.slnx --verify-no-changes; build = pnpm -C web build &&
             dotnet build api/Jobwright.slnx -c Release && the three docker builds; dev.sh starts PostgreSQL 17 and Keycloak 26
             containers on 127.0.0.1:55432 and 127.0.0.1:58080, then the API and the web app, and refuses to start while the
             k3d cluster runs, to stay within memory)
SCAFFOLD     create-next-app 16.4.5 — `pnpm create next-app@16.4.5 web --ts --tailwind --eslint --app --src-dir --import-alias "@/*" --use-pnpm --no-react-compiler --no-cache-components --no-agent-feedback --yes` — web/
               (flags from the create-next-app reference; Cache Components off because a nonce-based CSP needs dynamic rendering, ADR 0010)
             shadcn — `pnpm -C web dlx shadcn@4.23.0 init` then `add button card dropdown-menu sheet skeleton separator tooltip` — web/src/components/ui
             dotnet — `dotnet new sln -n Jobwright -o api` (slnx), `dotnet new webapi -n Jobwright.Api -o api/src/Jobwright.Api`,
               `dotnet new worker -n Jobwright.Worker -o api/src/Jobwright.Worker`, `dotnet new console -n Jobwright.Admin -o api/src/Jobwright.Admin`,
               `dotnet new classlib` for SharedKernel and each module and contracts project, `dotnet new install xunit.v3.templates::4.0.2`
               and `dotnet new xunit3` for each test project, `dotnet new tool-manifest && dotnet tool install dotnet-ef --version 10.0.14` — api/
             (slice 0 runs these and commits their output as an approved recipe commit; also `global.json` pinning SDK 10.0.403 with the test runner set to Microsoft Testing Platform)
CI           required, in order (ADR 0005; building-ci-pipelines), every one running a COMMANDS entry or a named tool:
             1 setup (pnpm install --frozen-lockfile with scripts blocked; dotnet restore --locked-mode) · 2 lint (check.sh lint, actionlint)
             3 unit (vitest; dotnet test --filter Category=Unit) · 4 contract (ContractDocumentTests, consumer tests, oasdiff from the second release)
             5 integration (dotnet test --filter Category=Integration against Testcontainers PostgreSQL 17; RowLevelSecurityTests; schema check;
             up-down-up migrations; AC25 from the second release) · 6 scans (gitleaks; trivy fs on lock files)
             7 build once (three images; digests recorded; syft bill of materials; trivy image) · 8 browser (Playwright against the built
             images with a test Keycloak: AC1 to AC3, AC7 to AC12, AC14; axe) · 9 budget (AC26)
             10 author check (fails a pull request whose author is the only code owner of a changed path; ADR 0005)
             on push to main only: publish the images and update staging in jobwright-gitops (ADR 0006)

REPOSITORY   jobwright-gitops
STACK        Kubernetes manifests, Helm values and Argo CD Applications (YAML); Bash for scripts; Python 3 standard library for checks — ADR: adr-draft-1.md, adr-draft-6.md, adr-draft-7.md, adr-draft-8.md
DEPENDENCIES platform (cluster): k3d with its k3s image (9.4); Traefik chart (9.4); Gateway API standard-channel CRDs (9.4); Argo CD (9.4);
               Argo Rollouts (9.4); Prometheus, prometheus-community "prometheus" chart, server only (9.4, ADR 0007); Sealed Secrets
               controller and kubeseal (9.4); PostgreSQL 17 official image (9.1); Keycloak 26 image (9.1); Mailpit (9.1);
               curlimages/curl (the canary's load job, ADR 0006)
             machine (outside the cluster): Gitea, gitea/runner, docker:dind (ADR 0005); the k3d registry (registry image);
               mkcert (the local certificate authority, ADR 0008)
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
             that change environments/production/) · 5 no-cluster-reach (no API server answers from inside a job) · 6 author check (as in jobwright-app)
             on push to main: staging guard (fails visibly when a commit touching environments/staging/ was not made by jobwright-bot
             or names a digest no main build of jobwright-app produced; ADR 0005, R5)

APPROVED     Prashant Sharma, 2026-11-08, recorded on the work item with this file's version and commit
```

## Pinned versions

Read on the vendors' official pages and registries on 2026-11-08 (checking-official-docs). FACT unless marked. The
rule used: the newest stable release, except where it is a new major or minor line only days old and an older line is
still supported (then the older line, so a fresh release does not land in the first build). Renovate proposes every
later update as a pull request, after a minimum release age the plan sets.

| Component | Pin | Source | Note |
|---|---|---|---|
| Node.js | 26.13.0 (LTS "Lithium") | nodejs.org/dist/index.json | Active LTS since 2026-10-28, supported to 2029-04-30 |
| pnpm | 12.18.0 | registry.npmjs.org/pnpm | Pinned by `packageManager` |
| Next.js, eslint-config-next | 16.4.5 | registry.npmjs.org/next | `next lint` is removed in 16: lint is the ESLint CLI with flat config |
| React, React DOM | 19.3.1 | registry.npmjs.org/react | |
| TypeScript | 5.9.3 | registry.npmjs.org/typescript | Not 7.x: openapi-typescript's peer is `^5.x` |
| Tailwind CSS, @tailwindcss/postcss | 4.3.5 | registry.npmjs.org | |
| shadcn (CLI) | 4.23.0 | registry.npmjs.org/shadcn | Scaffolding only |
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
| xunit.v3, xunit.v3.templates | 4.0.2 | nuget.org | Microsoft Testing Platform v2, switched on for `dotnet test` in `global.json` |
| xunit.runner.visualstudio, Microsoft.NET.Test.Sdk | 4.0.1, 18.11.0 | nuget.org; xunit.net (v3, Microsoft Testing Platform) | Kept, as xUnit recommends, until the platform mode is proved; then removed by a plan step |
| Testcontainers.PostgreSql | 4.16.0 | nuget.org | |
| OpenTelemetry .NET | Extensions.Hosting 1.20.1, Exporter.OpenTelemetryProtocol 1.20.1, Instrumentation.AspNetCore 1.20.2, Instrumentation.Http 1.20.2; Npgsql.OpenTelemetry 10.0.3 | nuget.org | |
| Architecture tests | TngTech.ArchUnitNET.xUnitV3 0.13.5 | nuget.org; github.com/TNG/ArchUnitNET | Active, pre-1.0; NetArchTest is unmaintained (last release 2021). It restores against xunit.v3 4.x, but runtime support is UNKNOWN (U13): slice 0's smoke test proves it, and the fallback is a reflection test over project references with no library |
| Assertions | xUnit's own | — | FluentAssertions 8 needs a paid licence for commercial use; not used |
| PostgreSQL | 17.11, image `postgres:17.11` | postgresql.org; hub.docker.com/_/postgres | Official image, not Bitnami; supported to 2029-11-08 |
| Keycloak | 26.8.2, image `quay.io/keycloak/keycloak:26.8.2` | github.com/keycloak/keycloak/releases | Organizations are a supported feature |
| Mailpit | 1.33.1, image `axllent/mailpit:v1.33.1` | github.com/axllent/mailpit/releases | |
| k3d | 5.9.0, with `--image rancher/k3s:v1.37.2-k3s1` and registry `registry:3.1.2` set explicitly | github.com/k3d-io/k3d/releases; hub.docker.com/r/rancher/k3s; hub.docker.com/_/registry | k3d's own defaults are stale (registry:2); no documented compatibility limit for 1.37 (INFERENCE: current stable channel, which k3d tests); proved in slice 0 |
| Traefik | 3.7.17, chart 42.3.1 | github.com/traefik/traefik/releases; traefik.github.io/charts | Supports Gateway API v1.6.3; `crossProviderNamespaces` set explicitly (its documented default is contradictory) |
| Gateway API CRDs | v1.6.3, standard channel | doc.traefik.io (supported version) | Not 1.7.0, which Traefik does not yet support |
| Argo CD | 3.5.7, chart `argo-cd` 10.11.7 | github.com/argoproj/argo-cd/releases; argoproj.github.io/argo-helm | 3.6.x is days old; 3.5 is still patched |
| Argo Rollouts | 1.10.2, chart `argo-rollouts` 2.43.11 | github.com/argoproj/argo-rollouts/releases; argoproj.github.io/argo-helm | |
| Prometheus | 3.16.0, chart `prometheus` 29.45.1 | prometheus-community.github.io/helm-charts | Server only; not kube-prometheus-stack |
| Sealed Secrets | controller and kubeseal 0.41.2, chart 2.22.0 from bitnami-labs | bitnami.github.io/sealed-secrets | Not the frozen bitnami/charts copy (app 0.31.0) |
| Gitea | 28.3.0 | github.com/go-gitea/gitea/releases | Requires code-owner approval (ADR 0005) |
| Gitea runner | `gitea/runner` 5.4.1 | github.com/go-gitea/gitea/releases (runner repo) | |
| Renovate | 44.222.4 | registry.npmjs.org/renovate | |
| Trivy | 0.76.2 | github.com/aquasecurity/trivy/releases | Never 0.69.4 to 0.69.6 (malicious builds, GHSA-69fq-xp46-6x23); pinned by digest |
| Syft | 1.57.0 | github.com/anchore/syft/releases | |
| gitleaks | 8.30.1 | github.com/gitleaks/gitleaks/releases | Slow release pace; watched by Renovate |
| oasdiff | 1.36.0 | github.com/oasdiff/oasdiff/releases | |
| create-next-app | 16.4.5 | registry.npmjs.org; nextjs.org (create-next-app reference) | Scaffolding only |
| pg, @types/pg | 8.23.1 | registry.npmjs.org | |
| shadcn's runtime set | radix-ui 1.8.1, class-variance-authority 0.7.1, clsx 2.1.1, tailwind-merge 3.7.0, lucide-react 1.70.0, tw-animate-css 1.4.0 | registry.npmjs.org | |
| @types/node, @types/react | 26.6.6, 19.3.0 | registry.npmjs.org | |
| docker:dind | `docker:29.9.3-dind` | hub.docker.com/_/docker | The runner's own daemon (ADR 0005) |
| mkcert | 1.4.4 | github.com/FiloSottile/mkcert/releases | No release since 2022; small and stable; run once by the owner |
| curlimages/curl | 8.23.0 | hub.docker.com/r/curlimages/curl | The canary load job |
| shellcheck, actionlint, kubeconform, yq | 0.11.0, 1.7.12, 0.8.0, 4.54.2 | their GitHub releases | |
| helm (CLI, for rendering tests) | 4.3.1 | github.com/helm/helm/releases | A new major; the charts above are rendered with it in slice 0's render check |
| Deferred, not approved | object storage (MinIO's community edition is archived, with no images since 2025-10; an alternative is chosen in the increment that needs it), flagd and OpenFeature | github.com/minio/minio | |
