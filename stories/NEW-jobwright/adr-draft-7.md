# 0007 · The platform of increment 1, and its memory

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: the machine running out of memory, which takes the product and the lab down together
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09)
Decider: Prashant Sharma (tech lead, and owner of the machine) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3

## Context
Requirements 9.1 and 9.4 list the platform; 9.5 and E21 cap the cluster at 6 GB beside the lab. Increment 1 does not
need every component yet. The challenge (Challenged C10) estimated the full increment-1 platform at about 5.6 to 5.9
GB, unmeasured, with CI builds competing in the same VM.

## Drivers
- E21, AC28: at most 6 GB, measured idle.
- Requirements 9.5: Jobwright's ports never collide with the lab's.
- Staging and production both exist from this increment (spec A4).

## Options
A. One k3d cluster with one node; staging and production as namespaces; only what increment 1 uses; one Keycloak
   with a realm per environment and its own small database; measured in slice 0 against E21 — **chosen**.
B. Two clusters, one per environment — rejected: true separation of platform upgrades, but it doubles the platform
   (Traefik, Argo CD, Rollouts, Prometheus), far beyond 6 GB.
C. Everything of section 9 now (object storage, flags, live-update backplane) — rejected: memory for components no
   criterion of this increment uses.

## Decision
- **In the cluster, platform:** Traefik (1 replica), Argo CD (core only: no Dex, no notifications controller, no
  ApplicationSet), Argo Rollouts, Prometheus (server only, 24 hours of data), the Sealed Secrets controller,
  Keycloak (1 replica, realms `staging` and `production`), a small PostgreSQL instance for Keycloak only, Mailpit.
- **In the cluster, per environment:** one PostgreSQL 17 instance (the environment's single database), the web app,
  the API and the worker, one replica each, with a canary pod during a release.
- **Outside the cluster** (Docker containers, bound to 127.0.0.1): the registry, Gitea, the runner and its
  Docker-in-Docker daemon (ADR 0005).
- **Deferred** to the increment that first needs them: object storage (logos in increment 2, photos in increment 3),
  feature flags (the first flagged feature), the live-update backplane (increment 3).
- **Memory budget** (INFERENCE, to be measured; the challenge's estimate): k3s about 700 MB, Argo CD about 650, Prometheus
  about 400, Traefik, Rollouts and Sealed Secrets about 250, Keycloak about 1,300, the Keycloak database about 150,
  each environment about 900 (1,800), a canary and the migration job about 500: about 5.75 GB. Every workload has
  requests and limits set from these numbers.
- **Measured in slice 0** of `jobwright-gitops` with AC28's script. If the idle total exceeds 6 GB, slice 0 stops and
  the owner chooses what to cut; the build never trims a component on its own.
- **Ports on 127.0.0.1:** 8443 (HTTPS) and 8080 (redirects to HTTPS) for the gateway, 3030 for Gitea, 5050 for the
  registry. None is reserved by the lab (3000, 3001, 4318, 4444, 9090, 9091).
- **Sealed Secrets key:** exported once by the owner during setup and kept outside both repositories; deleting the
  cluster without it loses every sealed secret.

## Consequences
- More than 6 GB idle fails AC28's check — enforced in slice 0 and after every platform change.
- A port binding outside 127.0.0.1, or on a reserved port, fails AC28's port check — enforced.
- **Accepted risk R3** (accepted by Prashant Sharma, risk owner, 2026-11-08, **for the demo**; a known limit):
  staging and production share one cluster, one Traefik, one Keycloak
  and one platform; a platform change has no separate staging, and a Traefik restart, a node restart or the laptop
  sleeping stops both environments. Mitigation: platform changes go through a reviewed gitops pull request, and are
  first applied by the owner to a throwaway local cluster when memory allows.
- **Accepted risk R4** (accepted by Prashant Sharma, risk owner, 2026-11-08): a CI build can push the VM towards its
  limit while the cluster runs. Mitigation: one CI job at a time with a memory limit. Slice 0 measures the cluster twice,
  idle (AC28) and while a CI build runs, and stops if the cluster exceeds 6 GB in either; the build's own memory is
  recorded beside it, not counted in the 6 GB (reading confirmed by Prashant Sharma, 2026-11-08).
- Revisit when a deferred component arrives (its increment re-measures), or the measured total leaves less than 500 MB
  of headroom.
