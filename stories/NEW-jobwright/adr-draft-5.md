# 0005 · Where CI runs and how changes merge, before GitHub

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: a pre-merge job that can change the cluster or staging, or gates that are not enforced
- **Changed since the last version:** first version

Status: Superseded by 0011 (accepted 2026-11-09; superseded 2026-11-09, design amendment 1)
Decider: Prashant Sharma (tech lead, and risk owner for the accepted risks below) · Agreed: Prashant Sharma · Work item: NEW-jobwright · Tier: 3

## Context
Section 8 requires every gate to pass in CI on every change; 9.4 decides GitHub Actions syntax pinned to commit
SHAs, read-only tokens by default and Renovate; 9.2 forbids pushing to GitHub until the owner asks; 9.4 decides a
local Git server that Argo CD reads. Something on the machine must run the workflows and enforce the required checks
and the code owner's review. The challenge (Challenged C9, C10) found that a runner holding the Docker socket can
take over the cluster, and that a self-hosted Actions server differs from GitHub Actions. The version check found
that Forgejo cannot require a code owner's approval, and Gitea 28 can.

## Drivers
- AC24, AC27: required checks enforced by branch protection.
- Requirements 9.5: agents never change the cluster; only merged Git changes do.
- The same workflow files must run on GitHub later, unchanged.
- Memory: the cluster's 6 GB (E21) and the CI's builds share one Docker Desktop VM.

## Options
A. Gitea as the local Git server, with Gitea Actions and one `gitea/runner` whose jobs use a dedicated
   Docker-in-Docker daemon on its own network, never Docker Desktop's socket; actions resolved from github.com at
   pinned SHAs — **chosen**.
D. As A with Forgejo — rejected: Forgejo's code owners only request reviews; requiring a code owner's approval is an
   open request (Forgejo issue #5179), so the code-owner gate of section 8 could not be enforced.
B. No CI server: the person runs the checks with a local runner (`act`) or the commands, before merging — rejected:
   the cheapest, but nothing enforces a required check, so section 8 holds only by habit.
C. Push to private GitHub repositories now and use GitHub Actions — rejected for now: requirements 9.2. If the owner
   asks, the workflows move unchanged and this ADR is superseded.

## Decision
- **Gitea** (pinned in `new-system.md`) runs as a container outside the cluster, bound to 127.0.0.1 on port 3030 (3000
  is reserved for the lab). It hosts `jobwright-app` and `jobwright-gitops`; Argo CD reads `jobwright-gitops` from it.
- **Workflows** live in `.github/workflows/`, which Gitea also reads, so they move to GitHub unchanged.
  `[actions] DEFAULT_ACTIONS_URL = github` (Gitea's default, set explicitly; FACT: app.example.ini at v28.3.0), so `uses: owner/action@<sha> # vX.Y.Z` resolves exactly as on GitHub. No absolute-URL `uses:`.
- **Runner.** One `gitea/runner`, capacity 1 (one job at a time), with a memory limit; its jobs talk to a dedicated
  Docker-in-Docker daemon on a Docker network that has no route to the k3d network. No Docker Desktop socket, no
  kubeconfig and no cluster credentials are mounted into the runner or its jobs. Images are built in that daemon.
- **Secrets.** Only three: the registry push credential, the `jobwright-bot` token for `jobwright-gitops`, and the
  `renovate-bot` token (requirements 9.4: Renovate for updates; it may only open pull requests). The jobs that use them
  run only on `push` to `main` or on Renovate's schedule, guarded by `if:`; pull-request jobs get no secrets.
- **Least privilege.** Each workflow and job states `permissions:`; Gitea honours it for `GITEA_TOKEN` (FACT:
  docs.gitea.com/usage/actions/comparison), except GitHub-only scopes such as `statuses` and `id-token`, which the
  workflows do not use. The bots' own tokens are scoped too: `jobwright-bot` may push only to `jobwright-gitops`.
- **Branch protection** on `main` in both repositories: no direct push, no force push, required status checks (the
  list in `new-system.md`), and one approving review required from a code owner (Gitea's "Require approval from code
  owners"). The one exception is staging's versions: on `main` of `jobwright-gitops`, the unprotected file pattern
  `environments/staging/**` lets a push that changes only those files through without review (U14, below).
- **Accounts and their rights** (U14, decided by Prashant Sharma, 2026-11-08; none has cluster access):

  | Account | jobwright-app | jobwright-gitops |
  |---|---|---|
  | Prashant Sharma | code owner; approves | code owner; approves |
  | `aidlc-agent` | write; pushes `feat/*` branches only; opens every pull request | none: works from its own fork and opens pull requests from it, so it cannot use the staging exception |
  | `jobwright-bot` | none (its token is used by the app's CI job) | write; pushes `main` only for `environments/staging/**` |
  | `renovate-bot` | write; pushes `renovate/*` only | write; pushes `renovate/*` only |
  | ledger writer (`jobwright-ledger`, used by the cluster) | none | write; pushes `release-ledger` only |

  Branch-name limits are Gitea branch protection rules with push allowlists, one rule per pattern (`main`, `feat/*`,
  `renovate/*`, `release-ledger`), each allowlisting only the accounts above. Gitea also accepts a push of
  unprotected files from any account with write access (FACT, below), so the staging exception is open to every account
  with write access to `jobwright-gitops`; that is accepted risk R5.
- **The staging exception, checked** (FACT, read in Gitea's source, `routers/private/hook_pre_receive.go` and
  `services/pull/patch.go` on github.com/go-gitea/gitea; added in Gitea PR #16395): a push to a protected branch by an
  account outside the push allowlist is accepted when it is not a merge and every changed file matches an unprotected
  pattern; with no patterns, nothing is accepted; patterns are matched against lower-cased paths, so they are written
  in lower case. That the same code is in the pinned 28.3.0 is INFERENCE (the feature dates from 2021); slice 0 of
  `jobwright-gitops` shows a staging-only push accepted and a mixed push refused. The fallback the owner set, if it
  were not, was an auto-merging pull request from `jobwright-bot`; it is not needed.
- **Ordering and concurrency.** Required checks run cheapest first (building-ci-pipelines). The staging update job
  rebases and retries on a rejected push; one job at a time also serialises those updates.
- **Accepted risk R5** (accepted by Prashant Sharma, risk owner, 2026-11-09): an account with write access to
  `jobwright-gitops` (Prashant Sharma, `jobwright-bot`, `renovate-bot`, the ledger writer) can push a change to
  `environments/staging/**` on `main` without review, and Argo CD deploys it to staging. Production is unaffected: it
  still needs a reviewed pull request and the ledger check. Mitigation: `aidlc-agent` has no write access there (it
  works from a fork); each bot's token is stored only where it is used; and a CI job on every push to `main` fails,
  visibly, when a commit touching `environments/staging/` was not made by `jobwright-bot` or names a digest that no
  `main` build of `jobwright-app` produced.

## Consequences
- A pull request with a red or missing required check cannot merge — enforced by branch protection; each check is
  shown red once on purpose in its slice.
- No job can reach the cluster — enforced by the runner's network and mounts, checked by a CI job that fails if
  `kubectl` can reach any API server from inside a job.
- A tag or branch reference in `uses:` fails CI — enforced by a workflow lint step that requires a 40-character SHA.
- **Accepted risk R1** (accepted by Prashant Sharma, risk owner, 2026-11-08, **for the lab only**): the
  Docker-in-Docker daemon runs privileged in the same VM as the cluster; an escape from it could reach the cluster.
  Mitigation: no secrets or credentials in it, network isolation, and the person reads every workflow change before
  pushing it (aidlc-build). **Known limit:** this is not acceptable for any shared or production environment; such an
  environment needs a runner on a separate machine or VM, by a new ADR.
- **Accepted risk R2** (accepted by Prashant Sharma, risk owner, 2026-11-08, **with a condition**): a pushed branch can
  edit a workflow and remove the `if:` guard, and so reach the three secrets before review. Condition: only Prashant
  Sharma's account and the `aidlc-agent` account can push to the repositories, and each of CI's three secrets is
  scoped to the least it needs: registry push only, no cluster access. Mitigation as before: the person pushes;
  workflow files are code-owned; production still needs a reviewed pull request and the ledger check (ADR 0006).
  **Open (U14):** the condition as written leaves no push for the three bot accounts this design uses; see the design
  note.
- **Code owners and authors** (U6, decided by Prashant Sharma, 2026-11-08). In Gitea an author never approves their
  own pull request, and a code-owner rule whose only owner is the author is waived (FACT: Gitea v28.3.0,
  services/issue/pull.go). So: an `aidlc-agent` account opens every pull request; Prashant Sharma is the code owner
  and approves; a CI check fails a pull request whose author is the only code owner of a changed path. Prashant
  Sharma creates the `aidlc-agent` account and gives it push rights to feature branches only.
- Revisit when the repositories move to GitHub: secrets move to protected environments and `permissions:` is enforced.
