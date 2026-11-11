<!-- aidlc template: design-note -->
# NEW-jobwright · design note · amendment 2

## For the reviewer
- **Asked of:** Prashant Sharma, as tech lead, to approve ADR 0014 and the changes below
- **Record it:** On the APPROVED line below, and on this note's commit
- **Tier and main risk:** 3: the required-check list drifting from CI's jobs (ADR 0014)
- **Changed since the last version:** first version (2026-11-11)

Inputs: [spec v4](spec.md) (approved 2026-11-11), [amendment-2.md](amendment-2.md), the approved design, design
amendment 1 and ADRs 0001 to 0013. This note changes only what spec v4 changes. Everything else in the approved design
stands.

| Item | Change |
|---|---|
| ADR 0014 (new, [adr-draft-14.md](adr-draft-14.md)) | Required checks on `jobwright-app` `main` by job name, administrators bound, no review; `staging-release` environment limited to `main` holding `STAGING_TOKEN`; supersedes the matching parts of ADR 0011 |
| ADR 0012 | Amended by ADR 0014, one line: the release job declares `environment: staging-release` (not edited: ADRs are accepted) |
| `new-system.md` v7 | CI list: STAGING_TOKEN read through `environment: staging-release`; the required checks |
| Coordination plan v3 | OUTSIDE: the protection rule, the environment, the token as an environment secret, fork approval (U-A2), the AC27 drill; UNKNOWNS: U15's budget half answered for `jobwright-app`, U-A2 added; APPROVALS: KL1 narrowed |
| Threat model (design note) | Row "CI to jobwright-gitops": the token is reachable only from a `main` run naming the environment (was: any branch's workflow). Row "fork pull requests" (new): read-only token, no secret, approval if U-A2 |
| Contracts | None change: Released versions v2 and Release ledger v2 stand |
| Dependencies | None |

Unknowns: U-A2 (owner Prashant Sharma, his settings page; changes E33's wording only).

APPROVED     ______ (Prashant Sharma, tech lead)
