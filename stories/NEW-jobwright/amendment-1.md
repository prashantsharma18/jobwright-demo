# NEW-jobwright · amendment 1 · GitHub for repositories, images and CI

## For the reviewer
- **Asked of:** Prashant Sharma, as product owner, spec owner and risk owner, to decide each open item below; only then is spec v2 written
- **Record it:** On this file's pull request; then in spec v2's Amendments line
- **Tier and main risk:** 3: gates that cannot be enforced on the GitHub plan in use, or new credentials between the cluster and GitHub
- **Changed since the last version:** first version

Raised by Prashant Sharma (product owner, owner of the criteria), 2026-11-09, in the session. Against spec v1
(approved 2026-11-08). Requirements v1.0 sections 8, 9.2 and 9.4 change; the requirements file is the owner's, so its
v1.1 is a proposal below, not an edit.

## The change, as decided by the product owner

1. Requirements 9.2 and 9.4: `jobwright-app` and `jobwright-gitops` are pushed now to private GitHub repositories; no
   Gitea, no local Git server. Argo CD in the local cluster reads `jobwright-gitops` from GitHub with a read-only
   deploy key; images go to GitHub Container Registry and the cluster pulls them with a pull secret; both are sealed
   secrets.
2. Section 8: the quality gates run in GitHub Actions on every pull request; branch protection on `main` requires
   those checks and a code owner's review. A separate GitHub account, `aidlc-agent`, opens every pull request and
   Prashant Sharma approves (intent I3's known limit still applies).

**Classification:** a gap, not a misreading. Spec v1 is right for the requirements it was written against; the
requirements changed, so several criteria and edge cases need new wording and two new decisions.

## Facts checked (official sources, 2026-11-09)

| Label | Finding | Source |
|---|---|---|
| FACT | GitHub Free for a personal account gives private repositories "a limited feature set"; protected branches, required pull request reviewers and code owners are listed only under GitHub Pro for private repositories | docs.github.com/en/get-started/learning-about-github/githubs-plans |
| FACT | Included: Free $12 of Actions compute a month and 500 MB of Packages storage; Pro $18 and 2 GB | same page |
| FACT | Rulesets (including push rulesets that restrict file paths) are documented for GitHub Team and Enterprise plans; a bypass list can hold roles, teams or GitHub Apps | docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets |
| INFERENCE | Three images per merge to `main` (web, API, worker) reach 2 GB of private package storage within weeks without a retention rule | sizes of the base images; to be measured |
| INFERENCE | Gitea's "unprotected file pattern" (U14, R5) has no equivalent on a personal account; staging's automatic update needs another mechanism | the two facts above |

## Criteria, rules and edge cases this changes

| ID | Spec v1 says | What conflicts | Proposed for spec v2 | Decider |
|---|---|---|---|---|
| Rule (staging and production, 9.4) | "Staging updates by itself when a change merges; production changes only through a reviewed pull request in `jobwright-gitops`…" | Nothing in the words; the mechanism (Gitea's unprotected pattern) is gone | Unchanged wording; the mechanism goes to design (D-1 below) | — (design) |
| Rule (secrets) | "No secret is stored in either repository in readable form" | Unchanged, but a secret that is pushed is now on a remote at once, so CI finding it after the push is too late (E23) | Unchanged wording; E23 changes | Prashant Sharma (risk owner), via E23 |
| New rules | — | The amendment's decisions | Add, each "set by Prashant Sharma, amendment 1": both repositories are private on GitHub; Argo CD reads `jobwright-gitops` with a read-only deploy key; images live in GitHub Container Registry, pulled with a pull secret; the deploy key and pull secret are sealed secrets; the section 8 gates run in GitHub Actions on every pull request; branch protection on `main` requires those checks and a code owner's approval; `aidlc-agent` opens pull requests and Prashant Sharma approves | Decided |
| New rule, conflict | "`aidlc-agent` opens every pull request" | Three kinds of pull request are opened by automation in the approved design: Renovate's updates (requirement 9.4), the production promotion pull request (ADR 0006: "a person, or a 'propose promotion' workflow"), and staging updates if they become pull requests (D-1) | See decision A2 | Prashant Sharma (spec owner) |
| AC23 | "…its checks run… it fails, naming the version… a change merged to `jobwright-app` updates staging with no person acting" | The checks now run in GitHub Actions and the ledger lives on GitHub; the cluster must be able to write the ledger (a write credential from the cluster to GitHub), while the amendment gives Argo CD only a read-only key | Unchanged wording; the ledger's write path is decision A4 | Prashant Sharma (risk owner) |
| AC24 | "…When CI runs, Then it fails, naming the finding; every image built has a bill of materials…" | Unchanged; images are scanned before they are pushed to the registry | Unchanged | — |
| AC27 | "…lint… pass before it can merge" | Only enforceable if the GitHub plan has protected branches for private repositories (FACT above) | Unchanged wording; add assumption A5 below | Prashant Sharma (A1) |
| AC28 | Memory and ports of "the local cluster with every service of this increment" | Unchanged wording; Gitea, its runner, Docker-in-Docker and the local registry no longer run on the machine, so ports 3030 and 5050 and risk R4's "while a CI build runs" no longer apply | Unchanged wording | — (design) |
| AC20, AC21 | Rollout and rollback in steps | Unchanged; but a new pod now pulls its image from GitHub over the internet | Unchanged; E17 covers the failure | — |
| E17 | "The local registry or Git server is unavailable -> staging does not change, and nothing already running stops" | There is no local registry or Git server | **E17 v2:** "GitHub or its container registry is unavailable -> staging and production do not change; nothing already running stops; a pod restarted meanwhile starts from the image already on the node, or waits; nothing ever falls back to an unpinned image" | Prashant Sharma (release owner) |
| E23 | "A secret is found in a commit -> CI fails; the secret is treated as exposed and rotated, not only removed" | CI runs after the push, so the secret is already on GitHub | **E23 v2:** "…as now, and also: every commit is scanned on the machine before it is pushed, and a found secret stops the push" (whether GitHub's own push protection is also used depends on the plan: UNKNOWN U-A1) | Prashant Sharma (risk owner) |
| A5 (new assumption) | — | — | "The repositories belong to an account or organisation whose GitHub plan provides protected branches, required reviews and code owners for private repositories" | Prashant Sharma (A1) |

Unaffected (checked, no change): AC1 to AC19, AC22, AC25, AC26, AC29; E1 to E16, E18 to E22, E24 to E27; A1 to A4;
the intent (its outcome names no tool; its constraints point to section 9).

## New edge cases (proposed; none decided)

| ID | Condition -> proposed behaviour | Decider |
|---|---|---|
| E28 | The required checks cannot run (GitHub Actions down, or the month's included compute used up) -> nothing merges; branch protection also binds administrators, so no one merges around a missing check | Prashant Sharma (spec owner and risk owner) |
| E29 | The deploy key or the pull secret is revoked, expired or wrong -> Argo CD reports the application as unable to sync and changes nothing; running pods keep running; new pods wait; a release stops and rolls back by its analysis (E13); the key is replaced with a new sealed secret, never typed into the cluster | Prashant Sharma (risk owner) |
| E30 | The registry's storage limit is reached -> the image push fails and staging does not change; a retention job deletes images older than 30 days that no environment and no ledger entry names, and never one they name | Prashant Sharma (release owner) |
| E31 | A workflow on a branch other than `main` (including one `aidlc-agent` pushed) asks for a deployment secret -> it gets none: the secrets live in a GitHub environment that only `main` can use | Prashant Sharma (risk owner) |
| E32 | A pull request is opened by the code owner's own account -> it cannot be approved by its author, so it does not merge; the person reopens it from `aidlc-agent` | Prashant Sharma (spec owner) |

## Decisions needed before spec v2

| ID | Decision | Options and consequences |
|---|---|---|
| A1 | Which GitHub plan the repositories live under | (a) **GitHub Pro** on the personal account: protected branches, required reviews and code owners on private repositories (FACT); $18 of Actions compute and 2 GB of packages included. (b) **An organisation on GitHub Team**: the same, plus rulesets with bypass lists and push rulesets by path, which give staging's automatic update a clean mechanism (D-1). (c) **GitHub Free**: AC27's "before it can merge" and the code-owner gate cannot be enforced on private repositories; the amendment's section 8 decision would not hold |
| A2 | Who may open pull requests | (a) `aidlc-agent` opens every pull request a person or an agent writes; named automation opens its own: Renovate (9.4), the promotion workflow and, if D-1 needs it, `jobwright-bot` for staging; each still needs the checks, and every change except staging's versions needs your approval. (b) Literally every pull request from `aidlc-agent`: Renovate and the promotion workflow then run as `aidlc-agent`, which mixes an agent's account with automation and widens its token |
| A4 | How the cluster writes the release ledger to GitHub (AC23) | (a) A second, write-capable credential in the cluster, limited to the ledger (on a Team plan: a GitHub App allowed only on `release-ledger`; on Pro: a deploy key with write access, which can push any branch not protected). (b) The ledger is written by a GitHub Actions workflow from what Argo CD reports; that needs a read credential into the cluster from GitHub, which the design forbids (no job reaches the cluster). (c) The person records each completed rollout in the promotion pull request: no credential, but AC23's facts are then a person's statement |
| E17, E23, E28 to E32 | The edge cases above | Accept as proposed, or change |

## What the design must change (for `aidlc-design`, amendment mode, after spec v2)

Accepted ADRs are not edited; each changed decision gets a new ADR that supersedes it.

| Item | Change |
|---|---|
| ADR 0005 (CI before GitHub) | Superseded: GitHub Actions on GitHub-hosted runners; secrets in a GitHub environment limited to `main` (E31); branch protection with required checks, code-owner approval and administrators included (E28); CODEOWNERS rewritten as GitHub globs; `aidlc-agent` and the automation accounts per A2. Risk R1 (privileged daemon beside the cluster) and R4's CI half go away; R2 and R5 are replaced; U6, U12 and U14 are answered again for GitHub |
| ADR 0006 (delivery) | Revised by a new ADR: images pushed to GitHub Container Registry by digest; staging's automatic update without Gitea's unprotected pattern (D-1: on Team, a push ruleset for `environments/staging/**` with `jobwright-bot` as the only bypass; on Pro, an auto-merging pull request from `jobwright-bot` with a code-owner rule that its paths satisfy, to be checked); the ledger's write path per A4; registry retention (E30) |
| ADR 0007 (platform and memory) | Revised: Gitea, the runner, Docker-in-Docker and the local registry leave the machine; ports 3030 and 5050 are released; the memory budget gains their share; the cluster now depends on the internet for deploys and pulls (E17); Argo CD's repository credential and the image pull secret are sealed secrets |
| ADR 0001 (layout) | One line: `jobwright-gitops` is read from GitHub, not Gitea |
| ADR 0008 (gateway) | Unchanged, except the published-ports list loses 3030 and 5050 |
| ADRs 0002, 0003, 0004, 0009, 0010 | Unaffected |
| Design note | Threat model rows for CI to registry and gitops and for the cluster to GitHub (read key, ledger writer); Challenged C9, C10 and C12 marked superseded where GitHub removes them; the unknowns table |
| `new-system.md` | Remove Gitea, `gitea/runner`, `docker:dind`, the registry image; add GitHub-hosted runner labels, pinned; Renovate as the hosted app or a scheduled workflow (A2); GitHub Container Registry; actions pinned to commit SHAs (now on GitHub itself) |
| Coordination plan | OUTSIDE: create the private repositories, the accounts and the plan (A1), the deploy key and pull credential, branch protection; the release-ledger contract's writer (A4) |

## Everything that cites what changes (step 5)

| Artefact | Cites | State |
|---|---|---|
| `stories/NEW-jobwright/spec.md` | E17, E23, AC23, AC27, rules | Approved v1; becomes v2 after the decisions |
| `design-note.md`, ADR 0005, 0006, 0007, 0001, 0008, `new-system.md`, `coordination-plan.md` | ADR 0005's Gitea, U6, U12, U14, R1, R2, R4, R5, E17 | Approved 2026-11-09; changed by design amendment |
| `jobwright-app` on `main`: `AGENTS.md` (Gitea's CODEOWNERS format) and `CODEOWNERS` (Gitea regular expressions) | ADR 0005 | Merged by `aidlc-onboard` (prepare); the owner's files: a revision pull request, proposed for the owner |
| `jobwright-gitops` on `main`: `AGENTS.md` (Gitea in bootstrap, ports 3030 and 5050, the staging pattern and jobwright-bot's pushes) and `CODEOWNERS` (Gitea format, the unprotected pattern) | ADR 0005, ADR 0006, ADR 0007 | Merged by `aidlc-onboard` (prepare); a revision pull request, proposed for the owner |
| Requirements v1.0, sections 8, 9.2, 9.4 | — | The owner's file: proposal, a v1.1 recording this amendment |
| Tests, plans, task bundles, evidence | — | None exist yet |

**The plan returns for approval**, because the criteria change. No plan has been written, so nothing is re-planned yet.
Each part's state, for the tech lead to classify:

| Part | State | Proposed action |
|---|---|---|
| 1 `jobwright-app` slice 0 | Not started (its onboarding is merged) | Plans against spec v2, after the revision pull request to its `AGENTS.md` and `CODEOWNERS` |
| 2 `jobwright-gitops` slice 0 | Not started (its onboarding is merged) | Plans against spec v2, after the same revision there |
| 3 `jobwright-app` criteria | Not started | Plans against spec v2 |
| 4 `jobwright-gitops` criteria | Not started | Plans against spec v2 |

After the decisions: spec v2 (CURRENT raised to `spec v2`), then `aidlc-design` in amendment mode for the items above,
then the revision pull requests in each repository, then `aidlc-plan` for slice 0. Pushing the repositories to GitHub
is the person's step, after A1 is settled, so the protection exists before the first pull request.

## Decisions (Prashant Sharma, 2026-11-09)

| ID | Decision | Written into spec v2 |
|---|---|---|
| A1 | GitHub Free for now: the gates run and report in GitHub Actions on every pull request; nothing blocks a merge. Known limit; GitHub Pro later turns enforcement on | New rule; AC27; KL1 |
| A2 | No separate account: Prashant Sharma pushes branches and opens every pull request; author and approver are the same person (as intent I3). Named automation opens Renovate's updates and the production promotion | New rule; KL2 |
| A4 | Prashant Sharma records each completed or aborted rollout in the production promotion pull request; the cluster gets no write credential | New rule; AC23; KL3 |
| E17 v2, E23 v2, E28 to E32 | Accepted as proposed | Edge cases |

Conflicts this leaves, flagged in spec v2 for the spec owner (not resolved): E28 relies on branch protection, which A1
does not have; E31 relies on GitHub environments, which GitHub Free cannot configure for private repositories (FACT:
docs.github.com, managing environments); E32 relies on `aidlc-agent`, which A2 removes. Also checked (FACT,
docs.github.com): "Pull request authors cannot approve their own pull requests."

Superseded by these decisions: in "Decisions needed", A1's options (b) and (c), A2's options (a) and (b) as written,
A4's options (a) and (b); in "What the design must change", `aidlc-agent` and the ledger's write path (now the person's
record), and D-1 on Team (not chosen).

## Second round (Prashant Sharma, 2026-11-09)

E28 reworded (the person keeps the rule under KL1); E32 dropped; E31 decided: CI stores no deployment secret and uses
only GitHub's built-in token, read-only on pull requests and on `main` only to push images. Tier 3 review of the
amendment recorded. New conflicts, flagged in spec v2: E31 against AC23 (staging's automatic update needs a write to
`jobwright-gitops`) and against E30 (deleting old images). For design, not the spec: pull requests opened with GitHub's
built-in token do not start workflow runs (INFERENCE from GitHub's documented token behaviour, to confirm), so the
promotion pull request's checks run when the person pushes the rollout record to it (A4).

## Third round (Prashant Sharma, 2026-11-09)

E31 against AC23: (a), a scheduled workflow in `jobwright-gitops` commits staging's new image versions with its own
built-in token; a delay of a few minutes is fine. E31 against E30: (a), the retention job deletes images older than 30
days on `main` only; if the built-in token cannot, fallback (b). The v2 Tier 3 review covers AC29 and E27. Spec v2
approved by Prashant Sharma, recorded at his instruction.
