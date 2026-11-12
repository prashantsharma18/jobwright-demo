# NEW-jobwright · decisions after part 1 · 2026-11-11

```text
DECIDED BY  Claude, at Prashant Sharma's instruction ("take decision on my behalf which is appropriate and continue",
            2026-11-11); he may overturn any of them
SOURCES     jobwright-app plans/NEW-jobwright/part-1/onboard-confirm.md ("For Prashant Sharma", 1 to 3);
            jobwright-app plans/NEW-jobwright/part-1/security-review.md S2; feedback/2026-11-11-story-status-part-index;
            spec v3 KL1, E28; ADR 0011 (adr-draft-11.md line 75); coordination plan v2 OUTSIDE
```

| # | Question | Decision | Why | Who does it, where |
|---|---|---|---|---|
| 1 | Definition of done: add `scripts/check.sh browser` and `budget` for changes under `web/`? | **Yes.** For any change under `web/`, "must pass before you report done" becomes lint, test, browser and budget (browser and budget run in the pinned Playwright image, as AGENTS.md already shows). | AC12 and AC26 are enforced gates. Finding a failure first in CI costs a push, a red run and Actions minutes (U15), and part 3 is where most `web/` changes land. The only cost is a slower local run, and only for `web/` changes. | A jobwright-app session: AGENTS.md, in the follow-up pull request below |
| 2 | Branch protection on jobwright-app `main`, the six checks required? | **Yes, through spec amendment 2 before part 3 starts.** Require setup, lint, test, build, browser and budget on `main`, apply the rule to administrators, and require no approval (KL2). Move `STAGING_TOKEN` into an environment limited to `main` (S2). | E28 is kept only by hand today (KL1). The repository is public (E11), so Free offers both settings at no cost, and both can be undone in the settings. They change KL1, KL4 and ADR 0011, so they go through an amendment at the root and not straight into the repository. | Workspace root: [amendment-2.md](amendment-2.md) (drafted 2026-11-11, awaiting his approval), then aidlc-design (ADR 0014). After approval, Prashant Sharma changes the GitHub settings by hand |
| 3 | The `(CI also checks this)` wording | **Confirm the merged wording.** AGENTS.md on `main` (f99b998) already says "No CI check yet", "it checks nothing until a module has a type, D6" and "CI will check this: … planned for part 3 / with the chart". | It is true today. The slice that adds each check changes it to "CI also checks this". | Nothing to change; record the disposition in onboard-confirm.md (follow-up pull request) |
| 4 | Status script against part indexes: fix A, B or both | **A for this story; raise B with aidlc.** Part indexes get the stage rows `aidlc-plan`, `aidlc-build`, `aidlc-verify`, `aidlc-review` and `merged` (the onboard rows stay). Part 1's PINS reads `spec v3 · plan v4`. | Changing the script so that "contains merged" counts as merged would loosen the release gate. A needs no change to the tool, and part 2's index is about to be written anyway. B belongs in aidlc's template, for the next story. | Part 1: the jobwright-app follow-up pull request. Part 2: part 2's aidlc-plan. B: Prashant Sharma raises it with aidlc |
| 5 | jobwright-gitops on GitHub: private (spec) or public like jobwright-app (E11)? | **Private, as spec v3 says.** | Nothing needs it public. KL1 already accepts that a private repository on Free has no protection, and following the spec avoids a second deviation. | Prashant Sharma, by hand (coordination plan OUTSIDE) |
| 6 | `status.md` committed in da1ace3 | **Untrack it** until fix A has landed in part 1. | The feedback's "Until it is fixed" says not to commit it as a record of part 1's state, because it misreports part 1. | Workspace root (the commit that adds this file) |

## Follow-up pull request in jobwright-app (one, small)

Branch `docs/NEW-jobwright-part-1-decisions`. Prompt for a new session started in `jobwright-app/`:

> Read ../stories/NEW-jobwright/decisions-2026-11-11.md. In one pull request: (1) reshape
> plans/NEW-jobwright/part-1/README.md to fix A of ../feedback/2026-11-11-story-status-part-index/README.md: keep
> the `aidlc-onboard (prepare)` and `aidlc-onboard (confirm)` rows, add `aidlc-plan`, `aidlc-build`, `aidlc-verify`,
> `aidlc-review` and `merged` rows taken from the `slice 0` cell (merged: done, pull request #1, d612731), keep the
> long cell as history below the table, and make PINS read `spec v3 · plan v4`; (2) AGENTS.md: decision 1;
> (3) onboard-confirm.md: record decisions 1 to 3 as dispositions; (4) apply
> ../stories/NEW-jobwright/amendment-2.md's rows for jobwright-app: AGENTS.md's KL1 and STAGING_TOKEN wording, and
> PINS re-pinned to `spec v4 · plan v4` with a note that slice 0 is unaffected. Run the commands AGENTS.md lists.
> Stage explicit paths only (leave .claude/settings.json alone). Do not push.

After it merges, run `python3 scripts/story_status.py NEW-jobwright --write` at the root. Part 1 should read
`merged` with no needs.

## Prashant Sharma, by hand (before part 2's CI)

1. Create jobwright-gitops as a **private** GitHub repository and push `main`.
2. In jobwright-gitops, turn on "Allow GitHub Actions to create and approve pull requests".
3. Hold `STAGING_TOKEN` until amendment 2 is approved. Then create it (fine-grained, jobwright-gitops only,
   contents: write, at most one year) as an environment secret of `staging-release`, not a repository secret, and put
   its expiry in the calendar. No workflow reads it before part 3.
4. Raise fix B with aidlc (the onboard template and the status command disagree).
5. Do not change jobwright-app's branch settings until amendment 2 is approved.

## Part 2, slice 0

A new session started in `jobwright-gitops/`, beginning with `aidlc-start`. Its aidlc-plan writes part 2's index in
fix A's layout (stage rows and a PINS line).

## Results of the by-hand steps (reported by Prashant Sharma, 2026-11-11)

| Step | Result |
|---|---|
| jobwright-gitops on GitHub | Private repository created; `main` pushed over SSH as prashantsharma18 (the clone needed jobwright-app's per-repository `core.sshCommand`: the machine's default key logs in as another account); Actions may create and approve pull requests, default token read-only |
| Branch protection, jobwright-app `main` | Applied; `enforce_admins` true |
| Environment `staging-release` | Limited to `main`; holds `STAGING_TOKEN`; no repository secret exists |
| AC27 drill, pull request #3 | A deliberate type error made lint, build, browser and budget red (setup and test green); `gh pr merge` refused: "the base branch policy prohibits the merge". Closed unmerged, branch deleted. Reported check names are exactly `setup`, `lint`, `test`, `build`, `browser`, `budget`, equal to the required contexts. Last proof: the first green pull request merges |
| Follow-up pull request #4 | Merged 2026-11-12 (bc81899), all six checks green: a green pull request merges under the protection, the drill's last proof. `story_status.py` now reads part 1 as `merged`, pins `spec v4 · plan v4`, no needs |
| U-A2, fork approval | Answered: "Require approval for all external contributors" is offered on the personal public repository and was selected (was GitHub's default, first-time contributors). E33 stands as written |
| Found: global git hooks | `core.hooksPath=/home/psharma/lab/git-hooks` is set globally (the owner's lab; its branch-name warning shows `PAY-142`, not a jobwright file). Git then ignores each clone's `.git/hooks`, so the planned `scripts/install-hooks.sh` (E23 v2 pre-push gitleaks, ADR 0011) must set a local `core.hooksPath` or E23's push check never runs; for the plan of the slice that builds it |
