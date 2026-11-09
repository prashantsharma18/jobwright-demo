# Proposal · revision pull requests to the rules files (NEW-jobwright, design amendment 1)

For: Prashant Sharma, owner of `AGENTS.md` and `CODEOWNERS` in both repositories. These files were merged by
`aidlc-onboard` (prepare) under the approved Gitea design; spec v3 and ADRs 0011 to 0013 change what they must say.
They are the owner's files, so this is a proposal: the owner applies it, one pull request per repository, on a branch
`chore/NEW-jobwright-amendment-1`. Each line below is the whole line as it stands on `main` and as proposed.

## jobwright-app

### CODEOWNERS (whole file)
GitHub reads `CODEOWNERS` from the root, `.github/` or `docs/`, with gitignore-style patterns, not Gitea's regular
expressions. On GitHub Free a private repository does not enforce it (KL1); it still names the owner.

Now:
```
# Every change needs an approving review from its code owner (ADR 0005: Gitea's "Require approval from code owners").
# Gitea reads this file at the repository root and matches paths with Go regular expressions, not globs.
# When the repository moves to GitHub, rewrite the rules as globs (`*`), since the two formats differ.
# One owner today (requirements 11). Add a rule per path when a second owner joins.
.* @prashantsharma18
```
Proposed:
```
# GitHub CODEOWNERS (gitignore-style patterns). On GitHub Free, private repositories do not enforce code owners
# (spec v3, KL1); moving to GitHub Pro turns it on (ADR 0011).
# One owner today (requirements 11). Add a rule per path when a second owner joins.
* @prashantsharma18
```

### AGENTS.md
| Line | Now | Proposed |
|---|---|---|
| 29 | `- Every change is reviewed in a pull request by a code owner who did not write it.` | `- Every change goes through a pull request, merged only when every check has run and passed (spec E28). Prashant Sharma authors and merges; GitHub Free enforces nothing (KL1, KL2).` |
| 71 | `  workflow and job states \`permissions:\`; pull-request jobs get no secrets (ADR 0005).` | `  workflow starts with \`permissions: contents: read\`; pull-request jobs get no secrets; only the main-branch run reads \`STAGING_TOKEN\` (ADR 0011).` |
| 126 | ``- Owners: `CODEOWNERS` (repository root, Gitea's regular-expression format)`` | ``- Owners: `CODEOWNERS` (repository root, GitHub's format; not enforced on Free, KL1)`` |
| new, after 71 | — | `- Images: built once on \`main\`, tested by digest, pushed to ghcr.io with \`sha-<commit>\`, then staging is committed, never backwards (Released versions v2, ADR 0012).` |

## jobwright-gitops

### CODEOWNERS (whole file)
Now: as `jobwright-app`'s, plus the two lines on the unprotected staging pattern.
Proposed: the same proposed file as `jobwright-app`'s (the staging pattern does not exist on GitHub; ADR 0012).

### AGENTS.md
| Line | Now | Proposed |
|---|---|---|
| 32 to 33 | `- Every change is reviewed in a pull request by a code owner who did not write it. The \`aidlc-agent\` account has no` / `  write access here: it opens pull requests from its own fork (ADR 0005).` | `- Every change goes through a pull request, merged only when every check has run and passed (spec E28), except the staging commits jobwright-app's run makes with \`STAGING_TOKEN\`. Prashant Sharma authors and merges; GitHub Free enforces nothing (KL1, KL2).` |
| 38 | `- \`bootstrap/\`  the owner's one-time setup: k3d cluster, Gitea, registry, runner, local certificate authority,` | `- \`bootstrap/\`  the owner's one-time setup: k3d cluster, local certificate authority, Argo CD's repository credential and the pull secrets (sealed),` |
| 45 | ``- `.github/workflows/`  CI, read by Gitea Actions and later by GitHub unchanged`` | ``- `.github/workflows/`  CI on GitHub Actions (ADR 0011), including the promotion workflow`` |
| 76 to 78 | `- The digests in \`environments/*/values.yaml\` are the Released versions contract v1, written by jobwright-app's CI;` / `  the ledger files on \`release-ledger\` are the Release ledger contract v1, written by the cluster. Changing either` / `  shape is a contract change (ADR 0006).` | `- The digests in \`environments/*/values.yaml\` are the Released versions contract v2, staging's written by jobwright-app's run; \`environments/rollouts.yaml\` is the Release ledger contract v2, appended by Prashant Sharma in each promotion pull request and never changed. Changing either shape is a contract change (ADR 0012).` |
| 81 | `  workflow and job states \`permissions:\`; pull-request jobs get no secrets (ADR 0005).` | `  workflow starts with \`permissions: contents: read\`; this repository stores no secret (ADR 0011).` |
| 93 | `- Enforced: production versions: a production pull request names only digests the release ledger proves, checked by` | `- Enforced (reports only on Free, KL1): production versions: the three production digests share one revision completed together in staging, or are an earlier production set, per \`environments/rollouts.yaml\`, checked by` |
| 108 to 109 | `- Never write to \`environments/staging/**\` digests or to the \`release-ledger\` branch, and never rewrite either:` / `  jobwright-bot and the cluster write them (ADR 0005, ADR 0006). (CI also checks this: the staging guard on \`main\`.)` | `- Never write \`environments/staging/values.yaml\`: jobwright-app's run writes it. Never change or remove an entry of \`environments/rollouts.yaml\`; only Prashant Sharma appends to it (ADR 0012). (CI also checks this: the values check and the promotion check.)` |
| 111 | `  ports: 8443 and 8080 (gateway), 3030 (Gitea), 5050 (registry) (requirements 9.5, ADR 0007).` | `  ports: 8443 and 8080 (gateway) (requirements 9.5, ADR 0013).` |
| 138 | ``- Owners: `CODEOWNERS` (repository root, Gitea's regular-expression format)`` | ``- Owners: `CODEOWNERS` (repository root, GitHub's format; not enforced on Free, KL1)`` |

Also, in both repositories, the line beginning `- Never push to any remote until the owner asks.`: the owner has
asked (spec v2). Proposed: `- Agents never push; Prashant Sharma pushes (spec v2, A2).` followed by the commit
identity line, unchanged.
