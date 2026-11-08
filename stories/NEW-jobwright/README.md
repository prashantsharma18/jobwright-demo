<!-- aidlc template: story-index -->
# NEW-jobwright · Jobwright version 1

```text
SOURCE     docs/requirements/2026-11-08-jobwright-requirements.md (v1.0, 2026-11-08, product owner Prashant Sharma); provisional key until the tracker is chosen (requirements Q1)
KIND       new system
ROUTE      G
LEVEL      workspace root
CURRENT    none
```

Scope: version 1, requirements sections 4 to 6, 8 and 9. The live dispatch board (section 7, FR-BOARD) is not part of
this story: it is a later story the owner starts, and version 1 contains none of it.

Each stage marks its own row when it ends: done, or `stopped: <reason>`. The build row reads doing while slices remain.
Once the code owner approves, the person marks the merged row done in the pull request's final commit, so it reaches
the default branch only through the merge. A re-plan or a revision resets the later rows to "to do".

Each step's pull request merges before the next starts.

| Stage | Runs in | Status |
|---|---|---|
| aidlc-intent | workspace root | to do |
| aidlc-specify | workspace root | to do |
| aidlc-design (decisions, `new-system.md`, coordination plan) | workspace root | to do |
| create the repositories the design's LAYOUT names (a person) | workspace root | to do |
| aidlc-onboard (add the repositories to WORKSPACE.md and the deny rules) | workspace root | to do |
| aidlc-onboard (prepare) | jobwright-app | to do |
| slice 0 | jobwright-app | to do |
| aidlc-onboard (confirm) | jobwright-app | to do |
| aidlc-onboard (prepare) | jobwright-gitops | to do |
| slice 0 | jobwright-gitops | to do |
| aidlc-onboard (confirm) | jobwright-gitops | to do |
| aidlc-release | workspace root | to do |
| aidlc-operate (on an alert) | workspace root | on event |
| aidlc-feedback (on repeated findings) | workspace root | on event |

parts: see `status.md`

The per-repository rows assume the two repositories decided in requirements section 9.2; `aidlc-design` confirms them
in its LAYOUT, and this index changes with it.
