# NEW-jobwright · decisions for the parallel run under aidlc 8.4.0 · 2026-11-23

```text
DECIDED BY  Prashant Sharma (product owner, tech lead, code owner), in the session, 2026-11-23; recorded by Claude
SOURCES     pending.md (IF-1, IF-1a, HK7, HK10, HK12, D1, D15, D16); spec v4, Rules (amendment 1, A2);
            aidlc 8.4.0, ADR 0028 (the AI's push leave) and ADR 0031 (a workspace story runs from its root)
```

| # | Question | Decision | What it changes | Who does it, where |
|---|---|---|---|---|
| 1 | How part 3 goes on | **Its plan merges first, then it runs as a lane.** The plan and its five bundles go to `main` exactly as approved (commit 592e003). The paused slice's work is committed on `feat/NEW-jobwright-part-3` so the main copy goes back to `main` and a lane can claim the branch | pending.md D15, IF-1 | Claude: branch `docs/NEW-jobwright-part-3-plan` (pushed, 6cd4217) and commit 5b543f5 on the part's branch. Prashant Sharma opens and merges the plan's pull request |
| 2 | The health-version slice's integration and browser runs while the k3d cluster `jobwright` runs | **They may run while it runs.** Agents still never run k3d themselves | pending.md D1; the slice's progress note | Recorded in `progress-health-version.md` |
| 3 | Who pushes | **Agents may push their own feature branch, plainly: never `main`, a tag, or with force** (ADR 0028's leave). Prashant Sharma still opens and merges every pull request | spec v4's rule "Prashant Sharma pushes branches and opens every pull request" (amendment 1, A2), for agents' own feature branches only; AGENTS.md and `.claude/settings.json` in both repositories; this root's settings | Claude: one pull request per repository (`chore/NEW-jobwright-push-leave`); this root's settings in the commit that adds this file. Spec v4's line stays as it is until Prashant Sharma amends it |
| 4 | The root's per-repository deny rules | **Dropped; each repository's `.worktrees/` is allowed instead** (agreed 2026-11-22). A deny rule wins over any allow rule, so lanes could not write in their worktrees otherwise | this root's `.claude/settings.json` (commit a734674) | Done |
