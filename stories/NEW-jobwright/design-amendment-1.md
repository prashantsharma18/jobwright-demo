<!-- aidlc template: design-note -->
# NEW-jobwright · design note · amendment 1

## For the reviewer
- **Asked of:** Prashant Sharma, as tech lead, and as spec owner and risk owner for the items marked so; consumers approve the two contract versions
- **Record it:** On the APPROVED line below, and on this note's pull request with its version and commit
- **Tier and main risk:** 3: CI that stops when the month's included minutes run out, staging that moves backwards, or an image deleted while still in use
- **Changed since the last version:** first version

Inputs: [spec v2](spec.md) (approved 2026-11-09), [amendment-1.md](amendment-1.md), the approved
[design note](design-note.md) and ADRs 0001 to 0010. This note changes only what spec v2 changes; everything else in
the approved design stands.

```text
1 Does it change the system's shape?  yes — the local Git server, runner and registry go; GitHub, its registry and Actions come in
2 Does it move a boundary?  yes — credentials between the cluster and GitHub; who writes staging and the release ledger
3 Does it change a contract someone outside the team relies on?  no — both changed contracts have the same owner at both ends

OUTCOME  D11 CI on GitHub Actions (adr-draft-11, supersedes 0005) · D12 delivery through GitHub (adr-draft-12, revises 0006)
         D13 platform without a local Git server or registry (adr-draft-13, amends 0007, 0001, 0008) · D14 image retention
         (adr-draft-12; none automatic, P2) — decisions P1, P2, P4, R6 and R7 taken 2026-11-09 (spec v3)
UI REFERENCE  none: no new screen or flow
RECORDED BY  Prashant Sharma, 2026-11-09
APPROVED  ADRs 0011 to 0013 now Accepted (0005 and 0006 superseded; 0001, 0007, 0008 amended) · contracts Released versions v2 and Release ledger v2 and their tests · boundary rules in "Must-hold consequences" · approvers: Prashant Sharma (tech lead; provider and consumer of both contracts), 2026-11-09, recorded at his instruction · the v3 security review is not yet recorded (spec v3)
```

## Contracts (amendment step 1 and 2)

| Contract | v1 says | Finding | v2 | Kind | Parts that consume it |
|---|---|---|---|---|---|
| Released versions | The provider (`jobwright-app` CI) commits staging's digests to `jobwright-gitops` | E31: the provider's built-in token can write only its own repository and packages | [released-versions-v2.md](contract/released-versions-v2.md): the provider publishes `sha-<commit>`, moves `main`, and commits staging's versions with the staging token (P1), never backwards. Tests: [test_published_images.py](contract/tests/released-versions/test_published_images.py), [test_staging_commit.py](contract/tests/released-versions/test_staging_commit.py) | major, by replacement (nothing implements v1) | parts 3 (provider) and 4 (consumer) |
| Release ledger | Written by the cluster on `release-ledger` | A4: the cluster has no write credential; the person records rollouts | [rollout-records-v2.md](contract/rollout-records-v2.md): `environments/rollouts.yaml`, appended in each promotion pull request. Tests: [test_promotion_check.py](contract/tests/rollout-records/test_promotion_check.py) | major, by replacement | part 4 |
| Jobwright API | — | unaffected | v1 stands | — | — |

The status command shows no part pinned to an old version: no part has been planned (amendment step 4).

## Facts checked (official sources, 2026-11-09)

| Label | Finding | Source |
|---|---|---|
| FACT | The built-in token's permissions are limited to the repository of the workflow; events it triggers create no new workflow run, except `workflow_dispatch` and `repository_dispatch`; pull requests it opens start their runs in an approval-required state | docs.github.com, GITHUB_TOKEN |
| FACT | Deleting a package version needs admin on the package; the repository whose workflow published it has admin by default; another repository can be granted a role on a personal account's package | docs.github.com, deleting and restoring a package; package access control |
| FACT | Scheduled workflows: every 5 minutes at most, default branch only, may be delayed or dropped under load | docs.github.com, events that trigger workflows |
| FACT | Container registry storage and bandwidth are "currently free", with at least one month's notice; data transfer by Actions with the built-in token does not count | docs.github.com, GitHub Packages billing |
| FACT | GitHub Packages accepts only classic personal access tokens; a classic token reaches everything the account can | docs.github.com, Container registry; personal access tokens |
| FACT | Actions minutes are rounded up per job; Free includes $12 a month, about 2,000 minutes of a 2-core Linux runner at $0.006 a minute; without a payment method, use is blocked when it is spent | docs.github.com, Actions runner pricing; Actions billing (via the challenge) |
| FACT | "Allow GitHub Actions to create and approve pull requests" is off by default for a personal account's new repository; the built-in token's default there is read-only for contents and packages | docs.github.com, managing GitHub Actions settings |
| FACT | The Renovate GitHub App's free tier covers private repositories on personal accounts (one job at a time, every 4 hours) | docs.renovatebot.com, Mend-hosted overview |

## Challenged (stress-testing-decisions; a fresh reviewer briefed to oppose D11 to D13)

| ID | Objection | Severity | Answer |
|---|---|---|---|
| C14 | A 5-minute poller is ~8,640 billed minutes a month, about four times Free's ~2,000; when they run out, every workflow stops, PR checks included | blocker | Decided P1 (a): no poller; `jobwright-app`'s main-branch run commits staging's versions with one stored token (ADR 0011, 0012; KL4) |
| C15 | Two merges in one window: the `main` tags can end on the older revision, and the poller commits a downgrade, or on mixed revisions, and staging silently stops | blocker | Changed: one `concurrency` group for the main-branch workflow of `jobwright-app` (runs queue, never overlap); the provider writes its run number as a label; the updater never moves staging to a lower run number, and reports when the three tags stay mixed for more than three runs |
| C16 | Retention by age deletes images production, a rollback target or `:main` still name; it shows only on a reschedule or a new cluster | blocker | Decided P2 (b): no automatic deletion while the registry is free; by hand if billing starts (E30 v3) |
| C17 | The promotion check is per digest, so web from one revision with the API from another can pass although that pair never ran together; and it only proves what the same person typed | serious | Decided P4 (a): one revision for all three, completed together in staging (AC23 v3); the second half is KL3 |
| C18 | Staging commits made with the built-in token start no workflow, so nothing validates what Argo CD then deploys | serious | Answered by P1: commits made with the stored token do start workflows, so `jobwright-gitops`'s CI renders and validates each staging commit (reports; KL1) |
| C19 | Scheduled runs are delayed, dropped or bunched; bunched runs race on the push; nothing says staging is behind | serious | No longer applies: no scheduled workflow (P1); the main-branch run's concurrency group and run-number guard cover two merges close together (C15) |
| C20 | The classic token reaches every package and repository the account can read; its expiry is the first failure | serious | R6 accepted for the lab (KL5); expiry in the owner's calendar (E29) |
| C21 | Renovate's app holds standing write access, and without branch protection could push to `main` | serious | Decided R7: Renovate off until GitHub Pro; dependencies updated by hand |
| C22 | No local fallback: offline or during a GitHub incident nothing syncs; a recreated cluster must pull everything | serious | Accepted already by E17 v2 (nothing running stops; new pods wait); a local pull-through cache is option B of ADR 0013, rejected for memory |
| C23 | Free's 500 MB of artifact storage fills with traces and reports | serious | Changed: artifacts kept 3 days; traces only for failed browser tests |
| C24 | Browser tests run against images built in the pull request, not the digests built on `main` | minor | Changed: the main-branch run tests the images it built, by digest, before it moves `main` |
| C25 | The pre-push hook can be skipped and is missing on a new clone | minor | Accepted as advisory: `scripts/install-hooks.sh` in each repository; CI's gitleaks still runs after the push (E23 v2) |
| C26 | Pinning by SHA does not pin what a composite action pulls by tag | minor | Changed: a workflow lint rule refuses `docker://` and composite actions that are not pinned within; the pinned list is in `new-system.md` |
| C27 | GitHub's SSH host-key rotation breaks Argo CD's known hosts; port 22 may be blocked | minor | Changed: Argo CD's known hosts come from GitHub's published keys, in the gitops repository; HTTPS with the same read-only key is the documented fallback if 22 is blocked |

## Decisions (taken by Prashant Sharma, 2026-11-09; spec v3)

Taken: P1 (a), with KL4 · P2 (b), E30 v3 · P4 (a), AC23 v3 · R6 accepted for the lab, KL5 · R7: Renovate off until
GitHub Pro. The options as they were put:

| ID | Question | Options and consequences |
|---|---|---|
| **P1** | How does staging update by itself within the Actions budget? (C14) | (a) **One stored, narrow token** (a new decision under E31): a fine-grained token with only `contents: write` on `jobwright-gitops`, used by `jobwright-app`'s main-branch run to commit staging's digests right after it publishes; no polling; about a minute after merge; costs no extra minutes. On Free it sits in repository secrets, readable by any workflow in `jobwright-app` (only you push there, A2), and it can write production's values too (no branch protection). (b) **A self-hosted runner on the machine for the updater only** (free minutes; every 5 minutes): a runner returns to the laptop, without Docker or any cluster credential. (c) **Poll every 2 hours** (~360 minutes a month): no secret and no runner, but staging lags up to about 2 hours, which changes AC23's "a few minutes". (d) **GitHub Pro** ($18 of minutes) does not make 5-minute polling affordable on its own |
| **P2** | Where does retention run, so it never deletes an image in use? (C16, E30, E31) | (a) **In `jobwright-gitops`**, which can read every environment and record: grant it the admin role on the three packages; it deletes versions older than 30 days that no environment, record or `main` tag names; this is a write (delete) by `jobwright-gitops` beyond E31's list, so E31 changes. (b) **No automatic retention while the registry is free** (FACT above): images are kept; when GitHub announces billing, you delete by hand what nothing names; E30 is reworded. (c) Keep it in `jobwright-app` by age only — rejected: it cannot see what is in use |
| **P4** | Should the promotion check require the three digests to be one revision that completed a staging rollout together? (C17) | (a) **Yes**: AC23 changes from "every digest recorded" to "the three digests share one revision, recorded as a completed staging rollout of all three, or a production set from an earlier promotion"; stricter, catches a mixed set. (b) **No**: AC23 stays per digest; a mixed set can pass |
| **R6** | Accept the classic token's reach (adr-draft-13)? | Accept with its mitigations, or not (there is no narrower token for packages) |
| **R7** | Accept that Renovate's app can write to `main` with nothing enforcing otherwise on Free? | Accept, or turn Renovate off until GitHub Pro (updates by hand) |

P1 (a) and P2 (a) change spec v2's E31 and P4 (a) its AC23, so each needs a spec v3 amendment line from the spec owner;
P1 (c) changes AC23's "a few minutes" the same way. P2 (b) rewords E30.

## Must-hold consequences and their checks (added by this amendment)

| Must hold | Check | Repository |
|---|---|---|
| The provider follows Released versions v2 | `test_published_images.py` (publish plan) | jobwright-app |
| Staging never moves to an older or mixed revision | `test_staging_commit.py` (the run-number guard, C15); the values check on each staging commit | jobwright-app; jobwright-gitops |
| Production changes only through a merged promotion pull request | a check on every push to `main` that reports a change to `environments/production/` not from one | jobwright-gitops |
| The promotion check follows Release ledger v2 | `test_promotion_check.py` | jobwright-gitops |
| Images by digest only | chart test; values check | both |
| Every action pinned, no unpinned composite or `docker://` | workflow lint (actionlint and the SHA rule) | both |

As before, the workspace root is read-only: each check is drafted here and shown red in the first step of its slice.

## Approval checklist (step 9)

| Item | State |
|---|---|
| Every decision has a Proposed record with a named decider | Yes: adr-draft-11, 12, 13, decider Prashant Sharma |
| Contract versions follow the rules and their consumers approved them | Both v2 are major, by replacement before any implementation; approval pending (same owner) |
| Every non-functional requirement has a check | Unchanged from the approved design, except AC23 and AC27 as spec v2 words them |
| Every must-hold constraint is enforced and seen to fail | Drafted; shown red in their slices |
| The map and rules file | Revision pull requests to both repositories' `AGENTS.md` and `CODEOWNERS`, drafted after approval |
| No decision left for the build | Yes, except U2, U9 and U13 of the approved design, still proved in slice 0, each with its fallback |
