# 0002 · Sign-in, sessions and the current tenant

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: a person acting in a tenant they do not belong to, or a session that outlives sign-out
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09)
Decider: Prashant Sharma (tech lead, and risk owner for E1 to E8) · Agreed: Prashant Sharma (security reviewer, known limit I3) · Work item: NEW-jobwright · Tier: 3

## Context
Requirements 9.1 decide Keycloak 26, one organization per tenant, OpenID Connect with PKCE, and the web app as the
back end for the front end. NFR-SEC-01: the browser holds only an httpOnly, Secure session cookie. The spec adds
server-side sign-out (AC2), expiry (AC3, E1), removal (E2), several tenants (E3), safe return addresses (E4) and
behaviour when the identity provider or session store fails (E7, E8). The challenge (design note, Challenged C5)
found that a token scoped to one chosen organization is not reliably issued by Keycloak 26 (organization pinned at
login, claim included inconsistently).

## Drivers
- AC1, AC2, AC3, AC6, AC14, E1 to E8, E12.
- FR-IAM-02: the tenant must never come from a value a client can change without a check.
- No new store or service (memory, requirements 9.5).

## Options
A. The web server holds the session and the current tenant; the access token proves who the person is; the web
   server sends the current tenant to the API in a header; the API accepts the header only from the web server and
   checks the person's membership and role in its own records on every request — **chosen**.
B. The token carries exactly one organization (the `organization:<alias>` scope); switching tenant asks for a new
   token; the API takes the tenant from the claim — rejected: relies on Keycloak behaviour the challenge found
   unreliable (pinned to the organization chosen at login; inconsistent claim), and switching needs a full sign-in
   round trip.
C. Tokens in an encrypted cookie, no server-side session — rejected: the cheapest, but sign-out cannot end the
   session on the server (AC2), and a stolen cookie works until expiry.

## Decision
- **Flow.** The web server runs the authorization code flow with PKCE (S256) and `state` and `nonce`, using
  `openid-client` (pinned in `new-system.md`), in the Node runtime only, never an Edge runtime. One confidential
  client `jobwright-web` per realm. Direct access grants and the implicit flow are off.
- **Session.** A random 256-bit id in the cookie `__Host-jw-session` (httpOnly, Secure, SameSite=Lax, Path=/, no
  Domain). The row in `web_sessions` holds the person, the current tenant, the tokens (encrypted with AES-256-GCM, key
  version stored per row, key from a sealed secret), `created_at`, `last_seen_at`. Idle 60 minutes and absolute 12
  hours are enforced on the server from these times (E1); `last_seen_at` is written at most once a minute. The web
  app's database role reaches `web_sessions` only.
- **Keycloak realm settings** match: SSO Session Idle at least 60 minutes and SSO Session Max 12 hours, so the identity
  provider never ends a session sooner than the product does; an audience mapper puts `jobwright-api` in the access
  token's `aud`.
- **Refresh.** When the access token is within 60 seconds of expiry, the request that needs it takes a row lock on its
  session, refreshes once, stores the new tokens and releases; concurrent requests wait for that refresh instead of
  refreshing too. A timeout or server error from Keycloak is retried once, then the request fails with
  `unavailable`; it never signs the person out (E7). An `invalid_grant` ends the session.
- **Sign-out (AC2).** Delete the session row, revoke the refresh token, then redirect the browser to Keycloak's end-
  session endpoint with `id_token_hint`, so Keycloak's own session cookie ends too; replaying the old cookie finds no
  row and is sent to sign in.
- **Current tenant.** At sign-in, a person with one membership gets that tenant; a person with several chooses (E3).
  The choice is stored in the session row, never in the browser. Switching tenant updates the row and reloads the
  page (E3). A person with no membership, or disabled, sees the no-access page (E6).
- **Web to API.** Every call carries `Authorization: Bearer <access token>` and `X-Jobwright-Tenant: <tenant id>`. The
  API validates the token's issuer, audience, signature and expiry; then looks up the person by the token's subject,
  and their membership in the header's tenant, in its own records, on every request. No membership: `403
  membership-revoked` and the web app ends the session (E2). A header for a tenant the person does not belong to, or no
  header: the same answer as no membership.
- **Only the web app reaches the API.** The API has no public route; it is reached through the gateway's internal
  listener (ADR 0006, ADR 0008), and network policies admit traffic to that listener only from the web app's pods,
  and to the API only from Traefik (k3s enforces network policies with its built-in controller; INFERENCE, checked in
  slice 0).
- **Return address (E4).** Only a path starting with a single `/` and resolving to a Jobwright page is honoured;
  anything else lands on Overview.
- **Back-channel.** The web server talks to Keycloak's token, end-session and certificate endpoints inside the cluster
  (Keycloak's back-channel hostname is dynamic), so these calls never pass the gateway's rate limit (ADR 0008); the
  issuer the tokens carry is still the public one.
- **Failures.** Session store unreachable: sent to sign in, never served without a session (E8). Identity provider
  unreachable: the sign-in-unavailable page; existing sessions keep working until their tokens can no longer be
  refreshed (E7).

## Consequences
- A token readable by scripts fails AC14's browser test — enforced (required check).
- A replayed cookie after sign-out, or past idle or absolute expiry, is refused — enforced by AC2 and AC3's tests,
  with a controlled clock.
- A request for a tenant the person does not belong to is refused — enforced by `RoleContractTests` (E2) and AC4.
- Two concurrent refreshes cause exactly one call to Keycloak — enforced by an integration test with a fake token
  endpoint that counts calls.
- A pod other than the web app cannot reach the API — enforced by a network-policy test in staging.
- Revisit if Keycloak's organization claim is proved reliable and a second client of the API appears; then option B
  may replace the header.
