# 0013 · The platform without a local Git server or registry

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design amendment; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: the cluster depending on the internet for every sync and pull, with long-lived credentials in it
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09, design amendment 1)
Decider: Prashant Sharma (tech lead, and owner of the machine) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3
Amends: 0007 (platform and memory), and one line each of 0001 (layout) and 0008 (published ports). Spec v2,
amendment 1.

## Context
Spec v2 moves both repositories to private GitHub repositories and the images to GitHub Container Registry. Gitea,
its runner, the Docker-in-Docker daemon and the local registry (ADR 0005, ADR 0007) no longer have a job on the
machine. The cluster now reads Git and pulls images over the internet.

## Drivers
- Spec v2 rules: a read-only deploy key for Argo CD, a pull secret for images, both sealed; the cluster holds no
  credential that can write to GitHub.
- E17 v2: GitHub or the registry down changes nothing and stops nothing.
- E21, AC28: at most 6 GB.
- E29: a revoked or expired key or pull secret stops syncs, never running workloads.

## Options
A. Remove the four machine components; Argo CD reads `jobwright-gitops` over SSH with a read-only deploy key; every
   namespace pulls from `ghcr.io` with one pull secret made from a classic personal access token with only
   `read:packages`; images by digest with `IfNotPresent`, so a node keeps what it pulled — **chosen**.
B. Keep a local registry as a pull-through cache of `ghcr.io` — rejected: survives an internet outage for images
   already cached, but it is a component and memory to run for a case E17 v2 already accepts (a new pod waits).

## Decision
- **Removed from the machine:** Gitea, `gitea/runner`, `docker:dind`, the k3d registry and their ports (3030, 5050).
  The published ports are 8443 and 8080 only (amends ADR 0008's list).
- **Argo CD's repository:** `jobwright-gitops` at `git@github.com:prashantsharma18/jobwright-gitops.git`, with a deploy
  key that has no write access, as a sealed secret labelled as an Argo CD repository credential. It polls every three
  minutes (Argo CD's default). (Amends ADR 0001: `jobwright-gitops` is read from GitHub.)
- **Image pulls:** a pull secret in each Jobwright namespace (staging, production), made from a classic personal access
  token of Prashant Sharma's with only `read:packages`, an expiry of at most one year, sealed. GitHub Packages accepts
  only classic personal access tokens for this (FACT: docs.github.com, working with the Container registry). Every
  workload names its image by digest with `imagePullPolicy: IfNotPresent`.
- **Rotation (E29):** both credentials are replaced by a new sealed secret through a reviewed gitops pull request,
  before expiry; the owner's calendar holds the token's expiry date. Argo CD and the kubelet show a failure; nothing
  running stops.
- **Memory:** the budget no longer includes the four machine components (they ran outside the cluster, so the
  cluster's 6 GB budget is unchanged); the VM gains their share. Risk R4's "while a CI build runs" no longer applies:
  CI runs on GitHub.

## Consequences
- A workload that names a tag instead of a digest fails CI — enforced by a chart test in `jobwright-app` and a values
  check in `jobwright-gitops`.
- A deploy key with write access is refused — advised by the setup runbook (GitHub shows the key's access; there is no
  check the cluster can run without a GitHub credential).
- **Accepted risk R6** (accepted by Prashant Sharma, risk owner, 2026-11-09, for the lab; spec v3 known limit KL5): the pull secret is a classic token with
  `read:packages` on every package Prashant Sharma can read, not only Jobwright's; anyone who reads that secret in the
  cluster can pull them. Mitigation: only `read:packages`, a one-year expiry, sealed, and readable only by the kubelet
  through the namespace's service accounts.
- Revisit if GitHub supports fine-grained tokens for packages, or the repositories move to an organisation, whose
  packages the token could then be limited to.
