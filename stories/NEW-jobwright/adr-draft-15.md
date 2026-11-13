# 0015 · The Kubernetes API on 127.0.0.1:6550, and what "the local cluster" means

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with design amendment 3
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: a cluster script that reaches a cluster other than the owner's local one (AC22, E24), or an API port open beyond the machine
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-13, design amendment 3; root change C3 of `jobwright-gitops`'s part 2 plan)
Decider: Prashant Sharma (tech lead, and owner of the machine) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3
Amends one line each of 0008 (published ports) and 0013 (published ports). Spec v4; no criterion changes.

## Context
ADR 0013 says "The published ports are 8443 and 8080 only", and `jobwright-gitops`'s AGENTS.md says "Jobwright's
ports: 8443 and 8080 (gateway)". But the owner's bootstrap and checks run kubectl, helm and kubeseal from the host, so
they need the Kubernetes API from the host, and k3d publishes it on its load balancer. Part 2's plan found the conflict
(challenge X10) and could not settle it on its own (root change C3). Its guard (AC22, E24) needs a fixed answer to "is
this the local cluster?", and the API's address is part of that answer.

## Drivers
- Requirements 9.5: every port bound to 127.0.0.1; the lab's reserved ports (3000, 3001, 4318, 4444, 9090, 9091) are not used.
- AC22, E24: every cluster script refuses any cluster but the local one.
- AC28: a port bound outside 127.0.0.1 fails the port check.
- E22: the setup names a taken port and stops.

## Options
A. Publish the API on 127.0.0.1:6550 as a Jobwright port, and make that exact server part of the guard's identity
   rule — **chosen**.
B. Keep the API unpublished and run every cluster command inside the k3d node container — rejected: helm, kubeseal and
   the measures would run in a container the guard cannot inspect, and the owner's scripts would carry two ways in.
C. Let k3d choose a random API port — rejected: the guard could not pin the server, and AC28's check could not name
   the expected set.

## Decision
- **Ports.** Jobwright's ports on the machine are exactly 6550 (the Kubernetes API), 8443 (HTTPS) and 8080 (redirects
  to HTTPS), each bound to 127.0.0.1 only. This replaces the port line of 0008 ("k3d publishes 8443 and 8080 on
  127.0.0.1 only") and of 0013 ("The published ports are 8443 and 8080 only"); neither file is edited, since both are
  accepted. AC28's port check (`scripts/check-ports.sh`) expects exactly this set on the containers labelled
  `k3d.cluster=jobwright`.
- **The local cluster (the guard's identity rule, K2 of part 2's plan).** A cluster script runs only when all of
  these hold, read from the file without contacting the cluster:
  - `KUBECONFIG` is exactly one file, mode 600, outside both repositories, not `~/.kube/config` and not a Windows-side
    `.kube/config`, compared by resolved path and by device and inode;
  - it holds exactly one context named `k3d-jobwright` and exactly one cluster of that name; the context's cluster
    reference resolves to it, and its server is exactly `https://127.0.0.1:6550`, with `certificate-authority-data`
    set and no `insecure-skip-tls-verify`, `proxy-url` or `tls-server-name`;
  - the context's user has client certificate data only (no exec, auth-provider, token or username);
  - the environment holds no `HELM_KUBE*` variable and no `DOCKER_HOST` or `DOCKER_CONTEXT`; the guard sets
    `KUBERC=off`; scripts that call docker or k3d also check that `docker context show` names the owner's Docker
    Desktop context;
  - every cluster command names `--context k3d-jobwright` (helm: `--kube-context`), never the current context.
  A refusal exits the script and names the file, the context and the server it refused.

## Consequences
- Another listener on 127.0.0.1:6550 fails the TLS handshake against the pinned certificate authority, and bootstrap
  stops on a taken port (E22) — enforced by the guard and create-cluster's port check.
- A binding outside 127.0.0.1, or a port outside the set, fails AC28's check — enforced by `scripts/check-ports.sh`
  and its tests (part 2).
- A kubeconfig that differs in any field above is refused — enforced by `scripts/test-guards.sh` (part 2), one fixture
  per condition.
- `jobwright-gitops`'s AGENTS.md ports line gains 6550 (the API, 127.0.0.1); that file is Prashant Sharma's to edit,
  with `aidlc-onboard (confirm)`.
- What runs today matches: the owner's run of 2026-11-13 showed k3d writing `https://127.0.0.1:6550` and check-ports
  reading "exactly 6550 8443 8080", all on 127.0.0.1 (part 2's progress-slice-0-bootstrap.md, step 8).

## Revisit
If a second cluster is ever run on this machine, or the cluster leaves it: the identity rule then needs another name
and port, and this ADR is superseded.
