# 0012 · Delivery through GitHub

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design amendment; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: staging moving to the wrong version, or production promoted on records that are wrong
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09, design amendment 1)
Decider: Prashant Sharma (tech lead, and release owner) · Agreed: Prashant Sharma (provider and consumer of Released versions v2 and the Release ledger v2) · Work item: NEW-jobwright · Tier: 3
Supersedes: 0006 (delivery). What 0006 decided about the canary, the Next.js skew, the analysis, schema changes and
rollback carries over unchanged and is restated below by heading. Spec v3, amendment 1.

## Context
Spec v3 moves images to GitHub's registry, lets one stored token commit staging's versions (P1), has the person record
each rollout in the promotion pull request (A4), requires the three production images to share one revision completed
together in staging (P4), and deletes no image automatically (P2). The challenge found that two merges could move
staging backwards (C15) and that staging commits would go unvalidated (C18).

## Drivers
- AC20, AC21, AC23 (v3), AC25; E13 to E15, E17 v2, E29, E30 v3; NFR-REL-02 (Decided).
- Contracts: Released versions v2, Release ledger v2 (both new versions, same owner at both ends).

## Options
A. `jobwright-app`'s main-branch run publishes the images, then commits staging's versions to `jobwright-gitops` with
   the staging token, never backwards; the person promotes production through a pull request the promotion workflow
   opens, adding the rollout records; no automatic retention — **chosen** (P1, A4, P2).
B. Argo CD Image Updater in the cluster, setting staging's images itself — rejected: staging's versions would then not
   live in Git, against requirement 9.4 ("Argo CD deploys only from Git"), and it is another component in the cluster.
C. A scheduled poller in `jobwright-gitops` — rejected: the Actions budget (C14).

## Decision
- **Publish** (Released versions v2): one run per merge (concurrency group); build the three images once; scan
  (fixable high or critical fails, AC24); bills of materials; browser tests against those digests (C24); push with
  `sha-<commit>` and the revision, source and run-number labels; move `main` on each, web, then API, then worker.
- **Staging by merge:** the same run writes `environments/staging/values.yaml` in `jobwright-gitops` (revision, run
  number, three digests) with the staging token and commits it to `main`; it changes no other file, refuses a lower run
  number than the file holds, and rebases and retries once if `main` moved. `jobwright-gitops`'s CI then renders and
  validates that commit (C18; reports only, KL1). Argo CD sees it within its poll (three minutes) and syncs staging,
  which rolls out by canary. Staging updates with no person acting (AC23).
- **Production by review:** Prashant Sharma starts the promotion workflow, which opens a pull request copying staging's
  three digests into `environments/production/values.yaml`. He appends to `environments/rollouts.yaml` each staging and
  production rollout he saw complete or abort since the last promotion (Release ledger v2), and pushes; that push runs
  `checks/promotion`, which passes only when the three digests share one revision completed together in staging, or
  are together an earlier production set (P4). He reviews and approves the pull request (opened by automation, so he
  can) and merges (KL1, KL3).
- **Retention:** none automatic while the registry is free (P2). When GitHub announces billing, Prashant Sharma deletes
  by hand the versions that no environment, no rollout record and no `main` tag names (E30 v3).
- **Unchanged from 0006:**
  - *Canary:* each workload an Argo Rollouts `Rollout` behind a sticky, weighted `TraefikService`; 20%, analysis,
    50%, analysis, 100% (U2 still proved in slice 0, fallback B needing the owner).
  - *Next.js skew:* `deploymentId` is the commit; the server-actions key is shared by all versions (a sealed secret).
  - *Analysis:* Traefik's per-service metrics; error rate and p95 with added buckets; at least 100 requests per
    measurement; three inconclusive measurements roll back (E15); a load job signing in as the probe tenant's person.
  - *Rollback:* an abort returns all traffic to stable at once; the person reverts the gitops commit so Git matches,
    and records the abort in the next promotion's rollout records.
  - *Schema:* PreSync migration job, idempotent, expand only, concurrent indexes outside a transaction with
    `lock_timeout`, a tested down path, and the previous release's tests against the new schema (AC25).
- **Outages:** GitHub or the registry down: staging and production do not change and nothing running stops (E17 v2).

## Consequences
- A staging commit that names a tag, mixes revisions or lowers the run number fails `jobwright-gitops`'s values check —
  enforced (reports; KL1); the publish step's own guard refuses it first — enforced by `test_staging_commit.py`.
- A promotion with a mixed set, or one not completed together in staging, fails `checks/promotion` — enforced by
  `test_promotion_check.py` (reports; KL1).
- The staging token can also write production's values (KL4); nothing on Free stops a mistaken or malicious write.
  Mitigation: only Prashant Sharma pushes to `jobwright-app` (A2), the publish step writes one file only, and
  `jobwright-gitops`'s CI on every push to `main` reports a change to `environments/production/` that did not come
  from a merged promotion pull request.
- **What cannot be undone:** a contraction step that drops data (unchanged from 0006); and a hand deletion of an image
  (restorable for 30 days through GitHub's interface only).
- Revisit on GitHub Pro: branch protection on `jobwright-gitops`'s `main` with the staging token's path the only
  direct push, and the token in an environment limited to `main`.
