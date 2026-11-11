# 0014 · Enforced gates and a main-only staging token in the public jobwright-app

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with design amendment 2
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: the required-check list drifting from the jobs CI runs, so a pull request waits forever or a new gate is not required
- **Changed since the last version:** first version

Status: Proposed (design amendment 2, 2026-11-11); accepted when Prashant Sharma approves design amendment 2
Decider: Prashant Sharma (tech lead) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3
Supersedes in part: 0011 (CI on GitHub): its "repository secret" for the staging token, its Consequence that every
check stops when the month's compute is spent (now `jobwright-gitops` only), and its "Revisit on the move to GitHub
Pro" for `jobwright-app`. Amends one line of 0012 (release). Spec v4, amendment 2.

## Context
`jobwright-app` is public (E11, 2026-11-11). GitHub Free gives a public repository protected branches and
environments with branch limits (docs.github.com, amendment 2's facts). Spec v4 requires its gates on `main`, binds
administrators, keeps no review (KL2), and keeps the staging token in an environment only `main` may use (E31 v4).
`jobwright-gitops` stays private; KL1 still applies there.

## Drivers
- E28 v4: no merge until every check has run and passed, held by GitHub in `jobwright-app`.
- E31 v4: the staging token is readable only by the one job on `main` that names `staging-release`.
- E33: fork pull requests get no secret, and need approval to run if GitHub offers it (U-A2).
- AC27 v4: each red gate is shown to block a merge in `jobwright-app`.

## Options
A. A classic branch protection rule on `main`: required status checks by job name, "Do not allow bypassing" on, no
   required review, "Require branches to be up to date" off; an environment `staging-release` with a deployment
   branch policy of `main` only, holding `STAGING_TOKEN` — **chosen**.
B. A repository ruleset instead of the classic rule — rejected for now: on a personal account a bypass list cannot
   hold actors (docs.github.com, about protected branches), and the classic rule does all spec v4 asks. Revisit if the
   repositories move to an organisation.
C. Keep the token a repository secret and rely on `if: github.ref == 'refs/heads/main'` in the workflow — rejected: a
   workflow on any branch can drop the condition and read the secret; only an environment enforces it outside the
   workflow file.

## Decision
A. The required checks are exactly the job names of `.github/workflows/ci.yml` that run on `pull_request`: today
`setup`, `lint`, `test`, `build`, `browser`, `budget`. Every job that runs on pull requests is unconditional at the
job level (no `paths` filter, no job-level `if:`), so a required check always reports. A slice that adds or renames a
pull-request job updates the rule in the same slice: the plan names the change, and Prashant Sharma applies it in the
settings before the slice's pull request merges. "Require branches to be up to date" stays off: one author, and it
would rerun every check after each merge to `main`.

The release job of ADR 0012 (part 3) declares `environment: staging-release`; it is the only job that does, and it runs
only on `push` to `main`. No other job, and no `pull_request` run, names the environment.

## Consequences
- A red or missing required check blocks the merge for everyone, Prashant Sharma included. Lifting the rule, even
  during a GitHub outage, is a new decision recorded in the story (E28 v4).
- Standard runners cost nothing on a public repository, so compute running out no longer stops `jobwright-app`'s
  checks; U15 informs speed there, not budget. `jobwright-gitops` keeps ADR 0011's consequence.
- A required check named after a job means renaming a job is a settings change too. Two jobs with the same name in two
  workflows would make the result ambiguous (docs.github.com); job names stay unique across workflows.
- KL4's first half (any branch could read the token) is gone; its second half (the token can write production's
  versions in `jobwright-gitops`) stays until GitHub Pro.
- The settings live in GitHub, not in a file: Prashant Sharma applies them by hand (coordination plan OUTSIDE), and
  AC27's drill shows they hold.

## Revisit
On the move to GitHub Pro or an organisation: protection for `jobwright-gitops`, a ruleset with a bypass list, and
code-owner review once a second person approves.
