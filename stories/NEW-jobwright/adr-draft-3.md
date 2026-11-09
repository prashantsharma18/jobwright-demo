# 0003 · Tenant isolation in the database

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: a query, a constraint or a background job that crosses tenants
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09)
Decider: Prashant Sharma (tech lead, and risk owner) · Agreed: Prashant Sharma (security reviewer, known limit I3) · Work item: NEW-jobwright · Tier: 3

## Context
FR-IAM-02 AC2 and requirements 9.1: one PostgreSQL database per deployment, a tenant on every row, isolation
enforced by the database, so a query that forgets the tenant still returns only the current tenant's rows (AC5) and
none when no tenant is set. The challenge (Challenged C1 to C4) found four ways a naive design breaks this: an
empty-string setting on pooled connections, constraints that ignore row-level security, migrations and views under
forced security, and queries outside the transaction.

## Drivers
- AC4, AC5, E5, E20.
- NFR-DATA-01: schema changes by expand and contract (ADR 0006).
- One database, one PostgreSQL instance per environment (requirements 9.1).

## Options
A. Row-level security on every tenant-owned table, forced, with the tenant set per transaction by the API; composite
   keys that include the tenant; separate roles for the application, the worker, the web sessions and migrations —
   **chosen**.
B. A schema per tenant — rejected: hundreds of schemas to migrate and a search path to get right on every connection;
   isolation then depends on the search path, which a query can name its way around.
C. Filtering in the application only (a global query filter) — rejected: the cheapest, but it is the "forgets to
   filter" case AC5 exists to catch.

## Decision
- **Tenant-owned tables** have `tenant_id uuid NOT NULL`, and `ENABLE` and `FORCE ROW LEVEL SECURITY` with one
  policy for all commands:
  `USING (tenant_id = nullif(current_setting('jobwright.tenant_id', true), '')::uuid)` and the same `WITH CHECK`.
  With no setting, or an empty one left on a pooled connection, the comparison is NULL and returns no rows; it never
  raises (Challenged C1).
- **Keys include the tenant** (Challenged C2): every primary key is `(tenant_id, id)`, every unique constraint starts
  with `tenant_id`, and every foreign key between tenant-owned tables is composite `(tenant_id, x_id)`, so a
  constraint can neither join two tenants nor reveal that another tenant's row exists.
- **Roles.** `jobwright_owner` owns the schema and runs migrations (it has `BYPASSRLS`, so a backfill sees every row;
  migrations are reviewed code, never a runtime path). `jobwright_app` (the API) and `jobwright_worker` are not owners,
  have no `BYPASSRLS` and get only the grants they need. `jobwright_web` has rights on `web_sessions` only.
- **Not tenant-owned:** `tenants` (id, alias, name), `persons` (id, subject, name, email), `web_sessions`.
  `jobwright_app` may select only `persons(id, subject)`; names and emails are read only through the
  `tenant_members` view, created `WITH (security_invoker = true)`, which joins `memberships`, so it obeys
  row-level security (Challenged C3). No `SECURITY DEFINER` function exists; a CI check fails if one is added.
- **Setting the tenant.** The API opens one short transaction per unit of work, never one per whole request, and an
  EF Core interceptor runs `SELECT set_config('jobwright.tenant_id', @tenant, true)` when the transaction starts
  (`TransactionStarted`, not `ConnectionOpened`). A command interceptor refuses to run any command on a tenant-scoped
  context outside a transaction or without the setting: it fails loudly instead of returning nothing (Challenged C4).
  Results are fully read before the transaction ends; no streaming past commit.
- **The worker** (Challenged C3) lists tenants from `tenants`, then works one tenant per transaction with the same
  setting. It has no cross-tenant query.
- **Pools.** The pool size of each part is set below the instance's `max_connections`, with headroom for migrations
  and the owner's tools; the numbers are in `new-system.md`.
- **Identifiers** from requests are parsed as UUIDs at the boundary; anything else is `invalid` before any query (E20).

## Consequences
- An unfiltered read sees only the current tenant, and nothing with no tenant — enforced by `RowLevelSecurityTests`
  (AC5), including on a reused pooled connection.
- A tenant-owned table without forced security, or without a test row in the isolation test, fails CI — enforced by
  `TenantOwnedTablesAreAllCovered`.
- A foreign key or unique constraint without `tenant_id` fails CI — enforced by a schema check run after migrations.
- A `SECURITY DEFINER` function, a view without `security_invoker`, or `BYPASSRLS` on a runtime role fails CI —
  enforced by the same schema check.
- A tenant-scoped command outside a transaction fails its test — enforced by the interceptor and its unit test.
- Revisit if a report must read across tenants (none in version 1); that needs a new ADR, never a bypass.
