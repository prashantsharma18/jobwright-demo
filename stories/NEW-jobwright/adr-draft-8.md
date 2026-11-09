# 0008 · The gateway, the client's address and rate limiting

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: sign-in attempts that are not limited per client, or a client that chooses its own address
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09)
Decider: Prashant Sharma (tech lead, and risk owner for E9 and E18) · Agreed: Prashant Sharma (security reviewer, known limit I3) · Work item: NEW-jobwright · Tier: 3

## Context
Requirements 9.4: traffic enters through the Gateway API, implemented by Traefik. NFR-SEC-04, AC16, E9, E18: sign-in
is limited to 10 attempts a minute per real client address, and a client cannot choose its address. The challenge
(Challenged C11) found that between the browser and Traefik the address is probably replaced: Docker Desktop's port
forward, k3d's load balancer (a TCP proxy) and the service's address translation. The version check confirmed it for
k3d's load balancer, an nginx stream proxy without PROXY protocol (FACT:
github.com/k3d-io/k3d/blob/v5.9.0/proxy/templates/nginx.tmpl). On this machine every client connects from the
machine itself.

## Drivers
- AC15, AC16, E9, E18, NFR-REL-04 (health never limited).
- Requirements 9.5: every port bound to 127.0.0.1.
- Secure cookies need HTTPS (ADR 0002).

## Options
A. Traefik's own chart with k3s's bundled Traefik turned off; HTTPS on `*.jobwright.localhost` with a local
   certificate authority; the limit keyed on the address Traefik sees, with no forwarded header trusted; what Traefik
   sees is measured in slice 0 and the result decides the rule below — **chosen**, subject to U1 and U3.
B. As A, but key the limit on a forwarded header set by a trusted front proxy (a tunnel for a public demo) — rejected
   for now: there is no front proxy in this design; it is the amendment if one is added.
C. Rate limiting inside the web app only — rejected: the cheapest, but Keycloak's own sign-in form would not be
   limited, and the limit would reset with each web pod.

## Decision
- **Gateway.** Traefik (pinned) with the Gateway API standard-channel CRDs at v1.6.3, the version Traefik 3.7 supports (FACT,
  doc.traefik.io), not the newer 1.7.0; one `GatewayClass`, one public
  `Gateway` with an HTTPS listener and an HTTP listener that only redirects, and one internal `Gateway` listener for
  web-to-API calls (ADR 0006) that is not published to the host and admits only the web app's pods.
- **Hosts.** `app.jobwright.localhost` (production), `app.staging.jobwright.localhost`, `auth.jobwright.localhost`
  (Keycloak, both realms), and operator hosts for Argo CD, Mailpit and Prometheus. Browsers resolve `.localhost` to
  the machine itself. The certificate comes from a local certificate authority the owner creates and trusts once
  during setup (tool pinned in `new-system.md`); its key is a sealed secret.
- **Published ports.** k3d publishes 8443 and 8080 on 127.0.0.1 only.
- **Forwarded headers.** No forwarded header from a client is trusted: Traefik's entry points trust no address for
  `X-Forwarded-*`, so a client-sent header is replaced, never used (E18).
- **Rate limit (E9).** A Traefik rate-limit middleware of 10 requests a minute per source address, attached to the
  sign-in routes: the web app's `/auth/*`, and Keycloak's `/realms/*/protocol/openid-connect/auth`, `/token` and
  `/realms/*/login-actions/*`. Refusals are `429` with `Retry-After` (as a problem response from the web app; Keycloak's
  own page for Keycloak's routes). (U3: Traefik documents middlewares as `ExtensionRef` filters with its Kubernetes CRD provider
  on, shown with AddPrefix; that RateLimit attaches the same way is confirmed in slice 0.) One Traefik replica, so the in-memory counters are exact; they reset on restart.
- **Keycloak** brute-force detection is on with temporary lockout only (never permanent), so an attacker cannot lock an
  account for good. Direct access grants are off.
- **Health.** Probes go from the kubelet straight to the pods; no health path is routed through the gateway, so none
  is limited (NFR-REL-04).
- **Client address (U1).** Slice 0 of `jobwright-gitops` measures the source address Traefik sees from two different
  clients on the host and from two pods. The rule then is:
  - If clients on the host are told apart: the limit keys on that address, as designed.
  - If they are not (expected: the load balancer replaces the address): every client on this machine counts as one
    client. On this deployment that is true:
    every real client is this machine. The limit then applies to the machine as a whole. AC16 is proved from two pods
    inside the cluster, which keep their own addresses (to be confirmed by the same measurement). **Accepted by
    Prashant Sharma, risk owner, 2026-11-08 (U1), as a known limit:** one sign-in limit for the whole machine is fine for
    the lab and the demo. Before any public tunnel or shared access, the rate-limit design changes first.
  - A public demo through a tunnel would make every client one again: it needs option B by design amendment, before
    the tunnel is added.

## Consequences
- A forged forwarded header changes nothing — enforced by AC16's test.
- An eleventh sign-in attempt within the minute is refused, and another address is not — enforced by AC16's test, from
  two pods.
- A port bound outside 127.0.0.1 fails AC28's port check — enforced.
- Revisit if a front proxy or tunnel is added (option B), or Traefik runs more than one replica (the counters then need
  a shared store).
