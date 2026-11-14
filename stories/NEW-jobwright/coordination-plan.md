# Coordination plan · NEW-jobwright · Jobwright: the new system and its foundations · v5

## For the reviewer
- **Asked of:** The tech lead or architect, to approve the parts, their order and the contract versions with the design; each part's own code owners approve its pull request
- **Record it:** On the work item with this plan's version and commit; each part in its own pull request
- **Tier and main risk:** 3: parts that land out of order, so a release depends on a platform piece that is not there
- **Changed since the last version:** v5 (2026-11-13, design amendment 4, from jobwright-app's part 3 plan v1 and its root-proposal.md; approved by Prashant Sharma, 2026-11-14): part 3 keeps its first pull request and P2 to P9 become parts 5 to 12 in jobwright-app; part 4 keeps the platform and its release slices become part 13, which waits for part 12's first release (D1); part 12 (the release job) waits for parts 3 and 5 to 11, part 4, and jobwright-gitops's review fixes with their security gate; STAGING_TOKEN deleted until part 12 (D7); Released versions v2.1 (D6); TRACKING, OUTSIDE, ORDER, VERIFY (AC25, D4a), UNKNOWNS (U16) and ROLLBACK follow; part 4 takes the errors middleware of D5. Before that, v4 (2026-11-13, design amendment 3, from part 2's root changes C1, C2, C3, C5, C6; approved by Prashant Sharma the same day): U1 and U2 move to part 4 with their scripts and U9 is answered (C1); bootstrap/ holds the cluster, Sealed Secrets, Argo CD's credential and Argo CD, while the certificate authority and the pull secrets move to part 4 (C2); 127.0.0.1:6550 joins Jobwright's ports (C3, ADR 0015); part 4's AppProject comes before part 3's release job (C5); kubectl 1.37.x (C6). Before that, v3 (2026-11-11, design amendment 2, spec v4; approved by Prashant Sharma the same day): jobwright-app public with required checks and the `staging-release` environment; OUTSIDE, UNKNOWNS and APPROVALS updated. Before that, v2 (2026-11-09, design amendment 1, spec v3): GitHub replaces Gitea and the local registry; Released versions v2 and Release ledger v2; OUTSIDE, UNKNOWNS, ROLLBACK and APPROVALS updated

```text
OUTCOME    Increment 1 of Jobwright (spec v1, AC1 to AC29) on a new system of two repositories. Tier 3 (security,
           tenant isolation, sign-in, release; spec v1)
CONTRACTS  Jobwright API v1: none -> 1.0.0 (new; api/contract/openapi.yaml in jobwright-app; consumer: the web app, same repository)
           Released versions (image digests by environment): none -> v2 (new; v1 withdrawn before any use; jobwright-app CI publishes to
           ghcr.io and commits staging's versions, jobwright-gitops reads; ADR 0012; contract/released-versions-v2.md)
           (v5, D6) Released versions v2 -> v2.1, additive: the first staging run creates an absent values file as run 0;
           provider part 12, consumer part 4 (its values check accepts a first file)
           Release ledger (rollout records): none -> v2 (new; v1 withdrawn; written by Prashant Sharma in each promotion pull request,
           read by checks/promotion; ADR 0012; contract/rollout-records-v2.md)
PARTS      #  Repository        Owner            Change                               Flag  Waits for                  Undo                        Status
           1  jobwright-app     Prashant Sharma  slice 0: skeleton, commands, CI      none  -                          revert the merge            waiting
           2  jobwright-gitops  Prashant Sharma  slice 0: skeleton, guard, measures   none  -                          revert the merge            waiting
           3  jobwright-app     Prashant Sharma  contract, problems, headers, health  none  parts 1, 2                 revert the merge            waiting
           4  jobwright-gitops  Prashant Sharma  platform: Traefik ... AppProject     none  part 2                     revert slice; Argo CD sync    waiting
           5  jobwright-app     Prashant Sharma  isolation in the database            none  part 3                     revert the merge (expand-only)  waiting
           6  jobwright-app     Prashant Sharma  API authorization, v1 operations     none  part 5                     revert the merge            waiting
           7  jobwright-app     Prashant Sharma  sign-in and sessions                 none  part 6                     revert the merge            waiting
           8  jobwright-app     Prashant Sharma  the shell and accessibility          none  part 7                     revert the merge            waiting
           9  jobwright-app     Prashant Sharma  telemetry                            none  part 3                     revert the merge            waiting
           10 jobwright-app     Prashant Sharma  the tenant command                   none  part 5                     revert the merge            waiting
           11 jobwright-app     Prashant Sharma  gates, images, chart                 none  part 3                     revert the merge            waiting
           12 jobwright-app     Prashant Sharma  the release job (STAGING_TOKEN)      none  parts 3, 4, 5 to 11; see note  revert; delete STAGING_TOKEN  waiting
           13 jobwright-gitops  Prashant Sharma  staging release, drill, AC16 e2e     none  part 4; part 12 first release  revert slice; Rollouts abort  waiting
           (v5, design amendment 4, D1: what each part proves.)
           (Part 3: AC13, AC15, AC17, AC18 (version), AC27 (part). Part 5: AC4 (keys), AC5, AC25, NFR-DATA-02.)
           (Part 6: AC4, AC6 (API), AC13, E2, E5, E20. Part 7: AC1 to AC3, AC14, AC16 (app share; the 429 route, D5), E1, E3, E4, E6 to E9, E12, E18, E25.)
           (Part 8: AC6 (offered), AC7 to AC12, AC26 on the Overview, E10, E11, E19. Part 9: AC18 (telemetry), AC19, E16, E26.)
           (Part 10: AC29, E27. Part 11: AC17 (probes), AC22, AC24, AC27 (gates drill), E23, E28, E33. Part 12: Released versions v2.1 provider, E28, E31, E33.)
           (Together AC1 to AC19, AC22, AC24 to AC27, AC29 and part 3 plan v1's E-items, as part 3 under v4.)
           (Part 4: AC16 (the limit), AC22, AC23, AC24, AC27, AC28. Part 13: AC20, AC21, AC16 end to end (U16). Together v4's part 4.)
           (Part 2's slice 0 also gives the owner the script that measures U9; part 4's Traefik and Rollouts slices, those of U1 and U2 (v4, C1).)
           (v5: part 12 also waits for jobwright-gitops's NEW-slice0-review-fixes merged with its security gate given: a manual gate, see ORDER.)
           (v5: part 12 is not planned before part 4 and that gate; part 3 plan K8.)
           (v5, D5: part 4's Traefik slice forwards a 429 on the web app's `/auth/*` routes to the web route `/errors/rate-limited`.)
           (v5, D5: the errors middleware runs before the rate limit; part 7 builds the route; the path changes only through aidlc-design.)
           (Status is derived: run the story status command named in WORKSPACE.md; it reads each part's index and writes `status.md` beside this plan.)
TRACKING   Branch feat/NEW-jobwright-part-<n> in each repository (slice 0: feat/NEW-jobwright-slice-0); pull request title
           "[NEW-jobwright] part <n> of 13: <what>". Every repository holds several parts, so each part's index is
           plans/NEW-jobwright/part-<n>/README.md: parts 1, 3 and 5 to 12 in jobwright-app, parts 2, 4 and 13 in jobwright-gitops.
           Each of parts 5 to 12 gets its index, plan and bundles from its own aidlc-plan, from part 3's plan v1 slice map (v5).
           The story stays one tracker item: parts are tracked here, never as more issues.
READ-ONLY  none (the workspace root reads both)
OUTSIDE    Prashant Sharma, by hand, never an agent: create both repositories (index row, done); create them on
           GitHub (jobwright-app public, E11; jobwright-gitops private) and push `main` (after this design is approved); in jobwright-gitops turn on "Allow GitHub
           Actions to create and approve pull requests"; create the staging token (fine-grained, jobwright-gitops only,
           contents: write, at most one year) as STAGING_TOKEN in jobwright-app's environment
           staging-release (v3); create the cluster's pull token
           (classic, read:packages only, at most one year) and the read-only deploy key of jobwright-gitops; put both
           expiry dates in the calendar; grant nothing else; run bootstrap/ once (v4, C2: the k3d cluster with the
           Kubernetes API on 127.0.0.1:6550, Sealed Secrets and its key export, Argo CD's repository credential as a
           sealed secret, Argo CD); seal the local certificate authority and the pull secrets with part 4's slices that
           first need them; run scripts/install-hooks.sh in each clone; run the tenant command (Apex, Brightline and the
           probe tenant); run the measurement of U9 with the script part 2 provides (done 2026-11-13), and those of U1
           and U2 with the scripts part 4's Traefik and Rollouts slices provide.
           (v4) Install kubectl 1.37.x under ~/.local/opt/kubectl-1.37.x for Jobwright's scripts, leaving the lab's
           /usr/local/bin/kubectl (v1.36.0) unchanged (C6). In jobwright-gitops's AGENTS.md, the ports line gains 6550
           (the Kubernetes API, 127.0.0.1), with aidlc-onboard (confirm) (C3, ADR 0015).
           (v3, amendment 2) jobwright-app is public (E11), jobwright-gitops private. In jobwright-app: a branch protection rule
           on `main` requiring setup, lint, test, build, browser and budget, "Do not allow bypassing" on, no review; the
           environment `staging-release` limited to `main`; STAGING_TOKEN created there as an environment secret, never a
           repository secret; fork-run approval for all outside contributors if offered (U-A2); a throwaway pull request with
           one red check to show the merge blocked (AC27). Each later slice that adds a pull-request job: add it to the rule.
           (v5) Before part 3 merges: add `integration` to the rule (part 3 plan Q4). After amendment 4 is approved, Prashant
           Sharma or the next part 3 session renames P2 to P9 as parts 5 to 12 in part 3's plan and index (jobwright-app).
           (v5, D7) Delete STAGING_TOKEN from jobwright-app's environment `staging-release` now, if it exists, and record
           in the story index that its secret list is empty; create it again (as v3 above) only when part 12's plan is
           approved with part 4 merged and jobwright-gitops's review fixes merged with their security gate given.
ORDER      Contract first: api/contract/openapi.yaml lands in part 3's first slice, with its failing tests, before any
           API code. Parts 1 and 2 in any order; aidlc-onboard (confirm) in each repository after its slice 0.
           (v5) In jobwright-app, one pull request at a time, each after aidlc-review (part 3 plan K10): part 3, then 5,
           6, 7, 8 in that order; 9 and 11 after part 3, and 10 after part 5, each placed between them by its readiness;
           part 12 last. Each part's aidlc-plan starts after the part it waits for has merged.
           (v5) Part 4 (the platform, with the AppProject) may run alongside parts 3 and 5 to 11, and merges before part 12.
           Part 12's first release before part 13: part 13 deploys the images part 12 publishes.
           (v4, C5) Part 4's AppProject, which limits what platform/ and environments/ may deploy, merges before part 12
           holds STAGING_TOKEN; until then risk R-k is open (design amendment 3), and no STAGING_TOKEN exists (D7).
           (v5) Manual gates the status script cannot see: jobwright-gitops's NEW-slice0-review-fixes merged and its
           security gate given (before part 12's plan); STAGING_TOKEN absent until then (D7). Prashant Sharma records
           each in the story index. `--check-release` governs aidlc-release (production); from part 12 on, every merge
           to jobwright-app's `main` releases to staging by design (ADR 0012), before the story is release-ready.
           Flags on: none. Feature flags are deferred (ADR 0007); a new system exposes nothing before its release.
VERIFY     Provider and consumer contract tests in jobwright-app CI (ADR 0004); oasdiff against the last release tag
           from the second release on; (v5, D4a) AC25: `main`'s head integration tests against each pull request's schema,
           from part 5's first migration (risk R-n, design amendment 4); the staging release drill (AC21) with a healthy and a deliberately failing
           version; checks/promotion with fixture pull requests (AC23); AC28's memory and port script.
UNKNOWNS   U1 client address: owner Prashant Sharma, answer in part 4's Traefik slice (v4, C1), blocks AC16 (parts 4 and 7 only)
           U16 (v5) whether the 11th sign-in attempt through Traefik returns 429 with application/problem+json and a Retry-After:
               owner Prashant Sharma, answer in part 13 end to end; blocks AC16's end-to-end evidence only (D5)
           U2 TraefikService routing and stickiness: owner Prashant Sharma, answer in part 4's Rollouts slice (v4, C1), blocks AC20, AC21
           (U6, U7, U12, U14 answered by spec v2 and v3: A2, A4, A1, P1)
           U-A2 fork-run approval setting: answered 2026-11-11, offered and selected; E33 stands
           (v3: U15's budget half is answered for jobwright-app: standard runners are free on a public repository; it stays for jobwright-gitops)
           U15 Actions minutes per pull request and per main-branch run: owner Prashant Sharma, measured in each slice 0, blocks
               the plan's monthly estimate (ADR 0011)
           U9 measured memory: answered 2026-11-13 (v4, C1): 1,746,025,841 bytes idle (k3s, Argo CD, Sealed Secrets), under
               6 GB; re-measured after every platform change (part 2's progress-slice-0-bootstrap.md, step 8)
           (U3, U4, U5, U8 are answered by documentation or the product owner before the slices that need them; design note)
ROLLBACK   Part 1, 2: revert the merge; nothing is deployed. (v5) Parts 3 and 5 to 11: revert the merge; until part 12
           nothing is deployed, and a revert that drops a required check's job also removes it from the rule (part 3 plan
           ROLLBACK). From part 12 on, a released change rolls back by Rollouts abort, then the gitops commit is reverted;
           schema changes are expand-only, so the previous version keeps working; the first contraction returns to
           aidlc-design (R-n). Parts 4 and 13: each slice reverts alone; Argo CD syncs the previous commit; the sealed-secrets
           key and `environments/rollouts.yaml` (which only grows) are never rewritten.
APPROVALS  (v3) In jobwright-app GitHub blocks a merge until every required check passes, administrators included (ADR 0014);
           in jobwright-gitops nothing is enforced (KL1).
           Each slice by Prashant Sharma, who also authors it (KL2: GitHub records no approval for an author; on Free
           nothing enforces the gates, KL1; he merges only when every check has passed, E28). Promotion pull requests,
           opened by automation, he approves. Plan: Prashant Sharma (tech lead). Specialist: security, privacy and release,
           Prashant Sharma (known limit, intent I3).
           Nobody approves or merges in another team's repository on its behalf.
```
