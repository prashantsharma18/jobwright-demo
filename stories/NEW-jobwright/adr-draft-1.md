# 0001 · Repositories, parts and module boundaries

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: modules that reach into each other's data, which makes isolation and later change unsafe
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09) · Amended by 0013 (2026-11-09)
Decider: Prashant Sharma (tech lead) · Agreed: Prashant Sharma (owner of both repositories) · Work item: NEW-jobwright · Tier: 3
Governs: `jobwright-app` (`docs/adr/`), with the layout of `jobwright-gitops` recorded here.

## Context
There is no code yet. Requirements 9.1 and 9.2 decide two repositories, one API service with clear internal modules,
a worker, and a web app that acts as the back end for the front end. What is left to decide is how the parts and
modules are laid out and how their boundaries are kept.

## Drivers
- Requirements 9.2: two repositories, `jobwright-app` (web, API, worker, Helm charts, decisions, rules) and
  `jobwright-gitops` (what runs where).
- Requirements 9.1: one API service with modules (jobs, scheduling, technicians, customers and sites, SLA,
  notifications, tenancy and identity), plus a worker.
- FR-IAM-02: isolation below the application; a module must not be able to read another module's tables directly.
- Spec AC22, AC23: scripts and production changes are guarded; agents never change the cluster.

## Options
A. Two repositories; inside the API, one project per module, each referencing only a shared kernel and other
   modules' `*.Contracts` projects, with an architecture test that fails on any other reference — **chosen**.
B. Two repositories; one API project with a folder per module and a naming convention — rejected: the cheapest
   option, but nothing stops a reference across modules, so the boundary holds only by review.
C. One repository for everything — rejected: requirements 9.2 decide two (Decided by the owner).

## Decision
- `jobwright-app` holds: `web/` (Next.js), `api/` (the .NET solution: `src/Jobwright.Api` host,
  `src/Jobwright.Worker` host, `src/Jobwright.Admin` console for the tenant command, `src/Modules/<Module>/` and
  `src/Modules/<Module>.Contracts/`, `src/Jobwright.SharedKernel`, `tests/`), `api/contract/openapi.yaml`
  (ADR 0004), `charts/jobwright/` (one Helm chart, three workloads), `docs/adr/`, `scripts/`.
- Increment 1 creates only the modules it uses: `Tenancy` (tenants, tenant settings, memberships, invitations) and
  `Identity` (persons, token validation, the current-tenant context). Each later module is created by the increment
  that first needs it; no empty projects.
- A module's tables belong to it alone. Another module reaches them only through that module's `*.Contracts`
  interfaces. Enforced by an architecture test in `tests/Jobwright.ArchitectureTests` (library pinned in
  `new-system.md`), which fails on any reference that is not to `SharedKernel` or a `*.Contracts` project.
- The worker is a separate process from the same solution and image build; in increment 1 it runs no jobs, only
  its health, version and telemetry (spec A3).
- `Jobwright.Admin` ships in the worker image and holds the tenant command (AC29). It is started only by the
  owner's guarded script (`scripts/create-tenant.sh` in `jobwright-gitops`, which refuses any cluster but the local
  one, AC22).
- Only the web app and the identity provider are reachable through the gateway's public listener. The API is reached
  only through an internal listener that admits the web app's pods (ADR 0006, ADR 0008); the worker has no route.
- `jobwright-gitops` holds: `platform/` (one Argo CD application per platform component, pinned),
  `environments/staging/` and `environments/production/` (the Jobwright chart's values, with image digests),
  `bootstrap/` (the one-time setup the owner runs), `scripts/` (every script guarded by the local-cluster check),
  `checks/` (the promotion check of AC23).

## Consequences
- A reference across modules fails CI — enforced by `tests/Jobwright.ArchitectureTests` (required check).
- A new tenant-owned table needs row-level security and an entry in the isolation test — enforced by
  `RowLevelSecurityTests.TenantOwnedTablesAreAllCovered` (ADR 0003).
- The API has no public route — enforced by a chart test that renders the chart and fails on any HTTPRoute that
  attaches the API or the worker to the public Gateway.
- Every script that touches a cluster calls the local-cluster guard first — enforced by a CI check in each
  repository that fails on a script calling `kubectl`, `helm` or `k3d` without sourcing the guard (AC22).
- Revisit if a module needs its own process (scale or isolation); that is a new ADR.
