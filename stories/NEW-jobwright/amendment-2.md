# NEW-jobwright · amendment 2 · jobwright-app is public: enforce its gates, fence its token

## For the reviewer
- **Asked of:** Prashant Sharma, as spec owner and risk owner, to approve the decisions below. Spec v4 is written only after that
- **Record it:** On this file's commit, then in spec v4's Amendments line
- **Tier and main risk:** 3: a gate that is said to block but does not, or a release token readable outside `main`
- **Changed since the last version:** v1 (2026-11-11); approved the same day, spec v4 written

Raised by the workspace root, 2026-11-11, from three sources: decision 2 in
[decisions-2026-11-11.md](decisions-2026-11-11.md) (taken by Claude at Prashant Sharma's instruction), jobwright-app's
E11, and finding S2 of jobwright-app's security review. Against spec v3 (approved 2026-11-09).

E11 was decided by Prashant Sharma on 2026-11-11 (jobwright-app `plans/NEW-jobwright/part-1/progress-slice-0-ci.md`):
github.com/prashantsharma18/jobwright-app is **public**. That already contradicts a spec v3 rule ("Both repositories
are private GitHub repositories"). It also removes the premise of KL1 and KL4 for that repository, because GitHub Free
gives public repositories the features those limits say are missing.

**Classification:** a gap. The spec is wrong about a decided fact (the visibility), and two of its known limits no
longer hold for one repository. No acceptance criterion's behaviour changes. What changes is the rules, E28, E31, KL1,
KL4 and AC27's verification.

## Facts checked (official sources, 2026-11-11)

| Label | Finding | Source |
|---|---|---|
| FACT | GitHub Free gives "unlimited public repositories with a full feature set"; protected branches, required reviewers and code owners for private repositories are listed only under GitHub Pro | docs.github.com/en/get-started/learning-about-github/githubs-plans |
| FACT | "Users with GitHub Free plans can only configure environments for public repositories." Environment secrets "are only available to workflow jobs that use the environment", after its protection rules pass; an environment can be limited to selected branches | docs.github.com/en/actions/how-tos/deploy/configure-and-manage-deployments/manage-environments |
| FACT | A branch protection rule binds administrators when "Do not allow bypassing the above settings" is on. Required checks "must have a `successful`, `skipped`, or `neutral` status" before a merge | docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches |
| FACT | Standard GitHub-hosted runners are free in public repositories and do not use the included compute | docs.github.com/en/billing/concepts/product-billing/github-actions |
| FACT | All six slice 0 jobs (setup, lint, test, build, browser, budget) run on every pull request, with no path filter and no job-level `if:`, so a required check cannot be left waiting by a change that skips it | jobwright-app `.github/workflows/ci.yml` on `main` (f99b998) |
| FACT | No workflow in jobwright-app reads `STAGING_TOKEN` yet; the first is part 3's release job (ADR 0012) | the same file; `grep` finds no use |
| INFERENCE | A required check that never reports (Actions down) keeps the pull request unmergeable; the page does not say so outright | the protected-branches page above |
| UNKNOWN | Whether a personal account's public repository offers "Require approval for all external contributors" for fork pull-request runs (S2 left it UNKNOWN) | U-A2 below |

## The change, as decided (decision 2, 2026-11-11)

1. jobwright-app is public (E11); jobwright-gitops stays private (decision 5).
2. On jobwright-app `main`, a branch protection rule requires the six slice 0 checks, plus each gate part 3 adds as it
   lands. It applies to administrators. It requires no approving review (an author cannot approve his own: KL2) and no
   code-owner review.
3. `STAGING_TOKEN` is an environment secret in a jobwright-app environment, `staging-release`, which only `main` may
   use. It is never a repository secret. Only the job that commits staging's versions names that environment.

## Criteria, rules and edge cases this changes (proposed for spec v4)

| ID | Spec v3 says | Proposed for spec v4 | Decider |
|---|---|---|---|
| Rule (repositories) | "Both repositories are private GitHub repositories; there is no local Git server" | "`jobwright-gitops` is a private GitHub repository. `jobwright-app` is a public one (E11): its code, plans and CI logs are world-readable, so, as the secrets rule already requires, neither holds a secret or personal data. There is no local Git server." | Prashant Sharma (E11, decided) |
| Rule (gates) | "…report pass or fail on it. Nothing blocks a merge on the current plan: known limit KL1" | "…report pass or fail on it. In `jobwright-app`, branch protection on `main` requires every gate that runs on pull requests, binds administrators, and requires no approving review (KL2). In `jobwright-gitops` nothing blocks a merge: KL1." | Prashant Sharma (spec owner) |
| Rule (the staging token) | "…kept as a repository secret in `jobwright-app`; `jobwright-app`'s run on `main` uses it…" | "…kept as an environment secret of `jobwright-app`'s environment `staging-release`, which only `main` may use; only the job that commits staging's versions names that environment…" (the rest unchanged) | Prashant Sharma (risk owner) |
| E28 | "…not merged until they pass; Prashant Sharma keeps this rule himself under KL1" | "…not merged until they pass. In `jobwright-app` branch protection holds this, administrators included; turning the rule off, even for an outage, is a new decision recorded in the story. In `jobwright-gitops` Prashant Sharma keeps it himself under KL1. (Compute running out no longer applies to `jobwright-app`: standard runners are free on a public repository.)" | Prashant Sharma (spec owner, risk owner) |
| E31 | "…used only by `jobwright-app`'s run on `main` to commit staging's versions…" | "…used only by the job of `jobwright-app`'s run on `main` that names the environment `staging-release`; a run on any other branch, or a job that does not name it, gets no secret…" | Prashant Sharma (risk owner) |
| E33 (new) | — | "A pull request to `jobwright-app` from a fork → its workflows get the read-only token and no secret. If GitHub offers the setting (U-A2), they wait until Prashant Sharma approves the run, for every outside contributor. Nothing from a fork is merged until a pull request with the same change, pushed by Prashant Sharma, has passed (A2 already has him open every pull request)." | Prashant Sharma (risk owner) |
| KL1 | "on GitHub Free, private repositories have no protected branches…; the gates report but block nothing" | "…this holds for `jobwright-gitops` only; `jobwright-app` is public and its gates block (rule above). Moving to GitHub Pro turns enforcement on in `jobwright-gitops`." | Prashant Sharma |
| KL4 | "the staging token is a repository secret, so any workflow in `jobwright-app` could read it, on any branch …; and it can write production's versions in `jobwright-gitops` too" | "the staging token can write production's versions in `jobwright-gitops` too, since on Free nothing protects that repository's `main`." (The first half no longer applies.) | Prashant Sharma |
| AC27 (verification) | "…enforcement after GitHub Pro (KL1)" | "…in `jobwright-app` each red gate is shown to block the merge; in `jobwright-gitops` enforcement waits for GitHub Pro (KL1)" | Prashant Sharma |

Unaffected (checked, no change): AC1 to AC26, AC28, AC29 (their behaviour); E1 to E27, E29, E30; KL2, KL3, KL5; A1
(still Free); A2, A4, P1 (one token; only where it is kept changes), P2, P4, R6, R7.

## Unknowns

| ID | Question | Owner | Blocks |
|---|---|---|---|
| U-A2 | Does a personal account's public repository offer "Require approval for all external contributors" (Settings > Actions)? If not, E33 keeps only its first and last sentences | Prashant Sharma, by looking in the settings | E33's wording only |

## What the design must change (aidlc-design, amendment mode, after spec v4)

Accepted ADRs are not edited. Each changed decision gets a new ADR that supersedes the part it changes.

| Item | Change |
|---|---|
| ADR 0011 (CI on GitHub) | Superseded in part by a new ADR 0014: the required checks and their list (kept in sync as part 3 adds gates), the `staging-release` environment, fork approval (E33), and the consequence "when the month's compute is spent every check stops", which now applies to `jobwright-gitops` only. U15 informs speed, not budget, for `jobwright-app` |
| ADR 0012 (release) | One line: the job that commits staging's versions declares `environment: staging-release` |
| `new-system.md` | CI section (line 105): STAGING_TOKEN as an environment secret, and the job that names the environment |
| Coordination plan | OUTSIDE: create the environment, put the token in it (not in repository secrets), set the protection rule and fork approval; APPROVALS: KL1 narrowed; UNKNOWNS: U15's budget half is answered for `jobwright-app` |
| jobwright-app `AGENTS.md` | "GitHub Free enforces nothing (KL1, KL2)" and the Workflows standard ("only the main-branch run reads `STAGING_TOKEN`"): reworded in the part 1 follow-up pull request |
| Requirements 9.2 | The owner's file: a v1.1 proposal records that `jobwright-app` is public |

## Parts, for the tech lead to classify

| Part | State | Proposed action |
|---|---|---|
| 1 jobwright-app slice 0 | Merged, pins spec v3 | Unaffected in code: no workflow reads the token, and the checks already run on every pull request. The part 1 follow-up pull request rewords AGENTS.md and re-pins PINS to `spec v4 · plan v4`, recording in the part index that slice 0 is unaffected |
| 2 jobwright-gitops slice 0 | Not started | Plans against spec v4 (nothing changes for it except KL1's wording) |
| 3 jobwright-app criteria | Not started | Plans against spec v4: the release job names `staging-release`; each new gate joins the required list in the same slice |
| 4 jobwright-gitops criteria | Not started | Plans against spec v4 |

## Order after approval

1. Spec v4 (CURRENT raised to `spec v4`), then aidlc-design in amendment mode (ADR 0014, the items above).
2. Prashant Sharma, by hand, in jobwright-app's settings: the branch protection rule for `main` (the six checks, "Do not
   allow bypassing", no required review); the environment `staging-release` limited to `main`, holding
   `STAGING_TOKEN`; fork approval if U-A2 says it exists. Then open a throwaway pull request with one red check to
   see the merge blocked (AC27).
3. The part 1 follow-up pull request in jobwright-app, now including the AGENTS.md rewording and the re-pin.

## Decisions (Prashant Sharma)

| ID | Decision | Written into spec v4 |
|---|---|---|
| Amendment 2 | Approved as proposed ("amendment 2 approved", 2026-11-11, workspace-root session) | Rules (repositories, gates, the staging token); AC27's criterion and verification; E28, E31, E33; KL1, KL4; U-A2 |

One wording follows from the approval and was not in the table above: AC27's criterion itself said "merging is not
blocked on the current plan (KL1)", so spec v4 words it to match its verification row (blocks in `jobwright-app`, not
in `jobwright-gitops`).
