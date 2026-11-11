# Coordination plan · NEW-jobwright · Jobwright: the new system and its foundations · v3

## For the reviewer
- **Asked of:** The tech lead or architect, to approve the parts, their order and the contract versions with the design; each part's own code owners approve its pull request
- **Record it:** On the work item with this plan's version and commit; each part in its own pull request
- **Tier and main risk:** 3: parts that land out of order, so a release depends on a platform piece that is not there
- **Changed since the last version:** v3 (2026-11-11, design amendment 2, spec v4; approved by Prashant Sharma the same day): jobwright-app public with required checks and the `staging-release` environment; OUTSIDE, UNKNOWNS and APPROVALS updated. Before that, v2 (2026-11-09, design amendment 1, spec v3): GitHub replaces Gitea and the local registry; Released versions v2 and Release ledger v2; OUTSIDE, UNKNOWNS, ROLLBACK and APPROVALS updated

```text
OUTCOME    Increment 1 of Jobwright (spec v1, AC1 to AC29) on a new system of two repositories. Tier 3 (security,
           tenant isolation, sign-in, release; spec v1)
CONTRACTS  Jobwright API v1: none -> 1.0.0 (new; api/contract/openapi.yaml in jobwright-app; consumer: the web app, same repository)
           Released versions (image digests by environment): none -> v2 (new; v1 withdrawn before any use; jobwright-app CI publishes to
           ghcr.io and commits staging's versions, jobwright-gitops reads; ADR 0012; contract/released-versions-v2.md)
           Release ledger (rollout records): none -> v2 (new; v1 withdrawn; written by Prashant Sharma in each promotion pull request,
           read by checks/promotion; ADR 0012; contract/rollout-records-v2.md)
PARTS      #  Repository        Owner            Change                               Flag  Waits for                  Undo                        Status
           1  jobwright-app     Prashant Sharma  slice 0: skeleton, commands, CI      none  -                          revert the merge            waiting
           2  jobwright-gitops  Prashant Sharma  slice 0: skeleton, guard, measures   none  -                          revert the merge            waiting
           3  jobwright-app     Prashant Sharma  increment 1 criteria (app)           none  parts 1, 2                 revert slice; Rollouts abort  waiting
           4  jobwright-gitops  Prashant Sharma  increment 1 criteria (platform)      none  part 2; part 3 first image  revert slice; Argo CD sync    waiting
           (Part 3 proves AC1 to AC19, AC22, AC24 to AC27 and AC29; part 4 proves AC16, AC20 to AC24, AC27 and AC28.)
           (Part 2's slice 0 also gives the owner the scripts that measure U1, U2, U7 and U9.)
           (Status is derived: run the story status command named in WORKSPACE.md; it reads each part's index and writes `status.md` beside this plan.)
TRACKING   Branch feat/NEW-jobwright-<part> in each repository (slice 0: feat/NEW-jobwright-slice-0); pull request title
           "[NEW-jobwright] part <n> of 4: <what>". Parts 1 and 3 are both in jobwright-app, so its indexes are
           plans/NEW-jobwright/part-1/README.md and part-3/README.md; likewise part-2 and part-4 in jobwright-gitops.
           The story stays one tracker item: parts are tracked here, never as more issues.
READ-ONLY  none (the workspace root reads both)
OUTSIDE    Prashant Sharma, by hand, never an agent: create both repositories (index row, done); create them on
           GitHub (jobwright-app public, E11; jobwright-gitops private) and push `main` (after this design is approved); in jobwright-gitops turn on "Allow GitHub
           Actions to create and approve pull requests"; create the staging token (fine-grained, jobwright-gitops only,
           contents: write, at most one year) as STAGING_TOKEN in jobwright-app's environment
           staging-release (v3); create the cluster's pull token
           (classic, read:packages only, at most one year) and the read-only deploy key of jobwright-gitops; put both
           expiry dates in the calendar; grant nothing else; run bootstrap/ once (k3d cluster, local certificate
           authority, Argo CD's repository credential and the pull secrets as sealed secrets, sealed-secrets key export);
           run scripts/install-hooks.sh in each clone; run the tenant command (Apex, Brightline and the probe tenant);
           run the measurements of U1, U2 and U9 with the scripts part 2 provides.
           (v3, amendment 2) jobwright-app is public (E11), jobwright-gitops private. In jobwright-app: a branch protection rule
           on `main` requiring setup, lint, test, build, browser and budget, "Do not allow bypassing" on, no review; the
           environment `staging-release` limited to `main`; STAGING_TOKEN created there as an environment secret, never a
           repository secret; fork-run approval for all outside contributors if offered (U-A2); a throwaway pull request with
           one red check to show the merge blocked (AC27). Each later slice that adds a pull-request job: add it to the rule.
ORDER      Contract first: api/contract/openapi.yaml lands in part 3's first slice, with its failing tests, before any
           API code. Parts 1 and 2 in any order; aidlc-onboard (confirm) in each repository after its slice 0.
           Part 3 before part 4's release slices: part 4 deploys the images part 3 builds. Part 4's platform
           slices (Traefik, Rollouts, Prometheus, Keycloak) may run alongside part 3.
           Flags on: none. Feature flags are deferred (ADR 0007); a new system exposes nothing before its release.
VERIFY     Provider and consumer contract tests in jobwright-app CI (ADR 0004); oasdiff against the last release tag
           from the second release on; the staging release drill (AC21) with a healthy and a deliberately failing
           version; checks/promotion with fixture pull requests (AC23); AC28's memory and port script.
UNKNOWNS   U1 client address: owner Prashant Sharma, answer at the end of part 2, blocks AC16 (part 3 and 4 slices only)
           U2 TraefikService routing and stickiness: owner Prashant Sharma, answer at the end of part 2, blocks AC20, AC21
           (U6, U7, U12, U14 answered by spec v2 and v3: A2, A4, A1, P1)
           U-A2 fork-run approval setting on a personal public repository: owner Prashant Sharma, his settings page, changes E33's wording only
           (v3: U15's budget half is answered for jobwright-app: standard runners are free on a public repository; it stays for jobwright-gitops)
           U15 Actions minutes per pull request and per main-branch run: owner Prashant Sharma, measured in each slice 0, blocks
               the plan's monthly estimate (ADR 0011)
           U9 measured memory: owner Prashant Sharma, answer at the end of part 2, blocks part 4's platform slices
           (U3, U4, U5, U8 are answered by documentation or the product owner before the slices that need them; design note)
ROLLBACK   Part 1, 2: revert the merge; nothing is deployed. Part 3: each slice reverts alone; a released slice rolls
           back by Rollouts abort, then the gitops commit is reverted; schema changes are expand-only, so the previous
           version keeps working. Part 4: each slice reverts alone; Argo CD syncs the previous commit; the sealed-secrets
           key and `environments/rollouts.yaml` (which only grows) are never rewritten.
APPROVALS  (v3) In jobwright-app GitHub blocks a merge until every required check passes, administrators included (ADR 0014);
           in jobwright-gitops nothing is enforced (KL1).
           Each slice by Prashant Sharma, who also authors it (KL2: GitHub records no approval for an author; on Free
           nothing enforces the gates, KL1; he merges only when every check has passed, E28). Promotion pull requests,
           opened by automation, he approves. Plan: Prashant Sharma (tech lead). Specialist: security, privacy and release,
           Prashant Sharma (known limit, intent I3).
           Nobody approves or merges in another team's repository on its behalf.
```
