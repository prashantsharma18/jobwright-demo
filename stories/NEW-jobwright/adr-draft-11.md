# 0011 · CI on GitHub Actions

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design amendment; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: gates that report but do not block (KL1), a stored token readable by any workflow (KL4), and a monthly budget that stops every check when spent
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09, design amendment 1)
Decider: Prashant Sharma (tech lead, and risk owner) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3
Supersedes: 0005 (where CI runs before GitHub). Spec v3, amendment 1.

## Context
Spec v2 and v3 move both repositories to private GitHub repositories on GitHub Free, run the section 8 gates in GitHub
Actions on every pull request, and allow exactly one stored secret. GitHub Free gives private repositories no branch
protection, required reviews, code owners or environments (FACT, docs.github.com), and about 2,000 minutes of a 2-core
Linux runner a month ($12 of included compute; FACT, docs.github.com, Actions billing), after which use is blocked.

## Drivers
- AC24, AC27, E23 v2, E28, E31 v3; known limits KL1, KL2, KL4.
- The month's included compute: every gate of every pull request must fit in it.
- Requirement 9.4: actions pinned to commit SHAs, read-only tokens by default. R7: Renovate off until GitHub Pro.

## Options
A. GitHub-hosted runners, every action pinned to a commit SHA, read-only by default, one stored token used only on
   `main`, no scheduled workflows — **chosen**.
B. A self-hosted runner on the machine for every job — rejected: free minutes, but it brings back a runner and a Docker
   daemon beside the cluster (the old R1), which the move to GitHub removed.
C. GitHub-hosted runners plus a scheduled staging poller — rejected: about 8,640 billed minutes a month on its own,
   four times the allowance (Challenged C14).

## Decision
- **Runners:** GitHub-hosted `ubuntu-24.04` (a fixed label, not `ubuntu-latest`).
- **Workflows** in `.github/workflows/` of each repository. Every `uses:` names a 40-character commit SHA with its
  version in a comment; no `docker://` reference and no unpinned composite action (workflow lint, Challenged C26). The
  pinned list is in `new-system.md`. actionlint runs as a pinned, checksum-verified binary (it is not an action).
- **Permissions:** every workflow starts with `permissions: contents: read`; a job asks for more only where this ADR
  says so.
- **Pull-request runs** (`pull_request`): the gates of section 8 that apply (spec S11), with the built-in token
  read-only and no secret. Each gate reports pass or fail on the pull request; nothing blocks a merge on Free (KL1).
  Prashant Sharma merges only when every check has run and passed (E28, his rule).
- **Main-branch run of `jobwright-app`** (`push` to `main`): one `concurrency` group (`main-release`, not cancelling a
  running job), so runs never overlap. It builds the three images once, scans them, writes their bills of materials,
  runs the browser tests against those digests, then pushes and tags them (`packages: write`) and commits staging's
  versions with the staging token (ADR 0012). The staging token is read only in this job, guarded by
  `if: github.event_name == 'push' && github.ref == 'refs/heads/main'`; on Free nothing stops another workflow from
  reading it (KL4).
- **The staging token:** a fine-grained personal access token of Prashant Sharma's, repository access
  `jobwright-gitops` only, permission `contents: write` only, an expiry of at most one year, stored as the repository
  secret `STAGING_TOKEN` in `jobwright-app` (P1). Its expiry date is in the owner's calendar.
- **`jobwright-gitops`'s runs:** its gates on every pull request and on every push to `main`, including the staging
  commits (which start workflows, because they are made with a stored token, not the built-in one). Its promotion
  workflow (`workflow_dispatch`) opens the production promotion pull request with the built-in token
  (`contents: write`, `pull-requests: write`); "Allow GitHub Actions to create and approve pull requests" is turned on in
  that repository only (FACT: off by default). The pull request's runs start approval-required (FACT); its checks run
  when Prashant Sharma pushes the rollout records to it (ADR 0012).
- **No scheduled workflow** in either repository.
- **Renovate is off** until GitHub Pro (R7). Prashant Sharma updates dependencies himself, as pull requests that pass
  the same gates.
- **Secrets on the machine:** `scripts/install-hooks.sh` in each repository installs a pre-push hook that runs the
  pinned gitleaks on what is about to be pushed and stops the push on a finding (E23 v2). CI's gitleaks still runs
  after the push.
- **Budget:** artifacts are kept 3 days; browser traces only for failed tests; caches keyed by the lock files. Slice 0
  of each repository measures the minutes one pull request and one main-branch run take, and the plan sets a monthly
  estimate from them.

## Consequences
- A tag or branch reference in `uses:`, a `docker://` image or an unpinned composite action fails the workflow lint —
  enforced (it reports; KL1).
- A job asking for more than this ADR allows is caught in review only — advised by this ADR and `AGENTS.md`.
- **When the month's included compute is spent, every check stops**, and by E28 nothing merges until the next month or
  until a payment method is added. This follows from A1 and E28 (both decided); the 90% and 100% usage emails from
  GitHub are the warning.
- Risks R1, R2, R4's CI half and R5 of ADR 0005 no longer apply; U6, U12 and U14 are answered by A2, A1 and P1.
- Revisit on the move to GitHub Pro: branch protection, required checks and code owners turn on, the staging token
  moves into an environment limited to `main`, and Renovate returns.
