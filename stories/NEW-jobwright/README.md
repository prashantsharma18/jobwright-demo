<!-- aidlc template: story-index -->
# NEW-jobwright · Jobwright: the new system and its foundations

```text
SOURCE     docs/requirements/2026-11-08-jobwright-requirements.md (v1.0, 2026-11-08, product owner Prashant Sharma); provisional key until the tracker is chosen (requirements Q1)
KIND       new system
ROUTE      G
LEVEL      workspace root
CURRENT    spec v4 · contract jobwright-api v1 · contract released-versions v2.1 · contract release-ledger v2
```

Scope: the new system and increment 1, Foundations (requirements section 4; product owner's answer I1 in
`intent.md`). Increments 2, 3 and 4 are later stories. The live dispatch board (section 7, FR-BOARD) is not part of
this story: it is a later story the owner starts, and nothing of it is built before.

Each stage marks its own row when it ends: done, or `stopped: <reason>`. The build row reads doing while slices remain.
Once the code owner approves, the person marks the merged row done in the pull request's final commit, so it reaches
the default branch only through the merge. A re-plan or a revision resets the later rows to "to do".

Each step's pull request merges before the next starts.

| Stage | Runs in | Status |
|---|---|---|
| aidlc-intent | workspace root | done |
| aidlc-specify | workspace root | done (spec v1 approved by Prashant Sharma, 2026-11-08) |
| aidlc-specify (amendment 1: GitHub for repositories, images and CI; `amendment-1.md`) | workspace root | done (spec v2 approved by Prashant Sharma, 2026-11-09, recorded at his instruction) |
| aidlc-design (decisions, `new-system.md`, coordination plan) | workspace root | done (approved by Prashant Sharma, 2026-11-09) |
| aidlc-specify (amendment 1, second part: spec v3) | workspace root | done (spec v3 approved by Prashant Sharma, 2026-11-09, recorded at his instruction; reviewed by him the same day) |
| aidlc-design (amendment 1: `design-amendment-1.md`) | workspace root | done (approved by Prashant Sharma, 2026-11-09, recorded at his instruction) |
| create the repositories the design's LAYOUT names (a person) | workspace root | done (Prashant Sharma, 2026-11-09: jobwright-app, jobwright-gitops) |
| aidlc-onboard (add the repositories to WORKSPACE.md and the deny rules) | workspace root | done (2026-11-09; deny rules already in `.claude/settings.json`) |
| aidlc-onboard (prepare) | jobwright-app | done (2026-11-09, merged to main as aba6d07) |
| slice 0 | jobwright-app | done (merged by Prashant Sharma through pull request #1, merge commit d612731, 2026-11-11; plan v4, commit 6936a41; `new-system.md` v6; see jobwright-app plans/NEW-jobwright/part-1/README.md; `status.md` reads it since pull request #4, fix A of feedback/2026-11-11-story-status-part-index) |
| aidlc-onboard (confirm) | jobwright-app | done (merged by Prashant Sharma through pull request #2, merge commit f99b998, 2026-11-11; every AGENTS.md command confirmed, docs/codebase-map.md written; its three decisions taken in `decisions-2026-11-11.md`) |
| decisions after part 1 (`decisions-2026-11-11.md`) | workspace root | done (2026-11-11, Claude at Prashant Sharma's instruction) |
| part 1 follow-up: part index to fix A, definition of done, dispositions | jobwright-app | done (merged by Prashant Sharma through pull request #4, merge commit bc81899, 2026-11-12; six checks green under the new protection; part 1 re-pinned to spec v4, unaffected) |
| aidlc-specify (amendment 2: branch protection and a `main`-only environment for jobwright-app; S2, E11), then aidlc-design (ADR 0011) | workspace root | aidlc-specify done (amendment 2 approved by Prashant Sharma, 2026-11-11; spec v4); aidlc-design done (design amendment 2, ADR 0014, `new-system.md` v7, coordination plan v3 approved by Prashant Sharma, 2026-11-11) |
| aidlc-onboard (prepare) | jobwright-gitops | done (2026-11-09, merged to main as 9e91986) |
| aidlc-design (amendment 3: part 2's root changes C1, C2, C3, C5, C6, decided by Prashant Sharma 2026-11-13; ADR 0015, `new-system.md` v8, coordination plan v4, `design-amendment-3.md`) | workspace root | done (approved by Prashant Sharma, 2026-11-13, recorded at his instruction) |
| slice 0 | jobwright-gitops | done (merged by Prashant Sharma through pull request #1, merge commit c7e4ef1, 2026-11-13; plan v1; U9 1,746,025,841 bytes idle, ports 6550, 8443 and 8080 on 127.0.0.1; merged before aidlc-review, which ran after the merge, pull request #2, 8895d3d; see jobwright-gitops plans/NEW-jobwright/part-2/README.md) |
| part 2 follow-up: the review's findings (2 high, 10 medium, 12 low, 10 questions, all deferred; security gate not given) | jobwright-gitops | to do: H2 decided 2026-11-13 (the sandbox stays off, risk accepted by Prashant Sharma); M1, M2, M3 and the security gate before part 3's release job holds STAGING_TOKEN (part 12's, coordination plan v5). Running as jobwright-gitops's own story NEW-slice0-review-fixes: design "none needed" 2026-11-13, plan v1 drafted |
| aidlc-onboard (confirm) | jobwright-gitops | done (merged by Prashant Sharma through pull request #3, merge commit 23f04b5, 2026-11-13; every AGENTS.md command confirmed, docs/codebase-map.md written, coverage of checks/ 87%) |
| aidlc-design (amendment 4: jobwright-app part 3's root-proposal.md: parts 3 and 5 to 12 in jobwright-app, part 4 split with part 13, YamlReader and Mailpit for tests, gates by project, AC25's job, the 429 route, Released versions v2.1, STAGING_TOKEN held back; `new-system.md` v9, coordination plan v5, `design-amendment-4.md`) | workspace root | done (drafted 2026-11-13 and challenged, 12 objections answered in the note; approved by Prashant Sharma, 2026-11-14, recorded at his instruction) |
| aidlc-release | workspace root | to do |
| aidlc-operate (on an alert) | workspace root | on event |
| aidlc-feedback (on repeated findings) | workspace root | on event |

parts: see `status.md`

The per-repository rows assume the two repositories decided in requirements section 9.2; `aidlc-design` confirms them
in its LAYOUT, and this index changes with it.
