# Contract · Release ledger · v2: the rollout records (draft, NEW-jobwright amendment 1)

Owner: Prashant Sharma. Writer: Prashant Sharma, by hand, in each production promotion pull request (spec v2, A4,
known limit KL3). Reader: `checks/promotion` in `jobwright-gitops` (AC23).

**Change from v1: major (breaking), done by replacement before any release.** v1 was written by the cluster on a
`release-ledger` branch. Under spec v2 the cluster holds no credential that can write to GitHub, so v2 is a file the
person writes. Nothing implements v1 yet; v1 is withdrawn.

## The file
`environments/rollouts.yaml` on `main` of `jobwright-gitops`. It only grows: a promotion pull request appends entries
and never changes or removes one.

```yaml
# One entry per rollout the person saw complete or abort, since the last promotion.
- environment: staging          # staging | production
  workload: web                 # web | api | worker
  digest: sha256:<64 hex>
  revision: <40-hex commit>
  result: completed             # completed | aborted
  at: 2026-11-20T10:42:00Z      # UTC, when the person saw the result
  seen_in: argo rollouts get rollout jobwright-web -n staging   # how the person saw it
```

## What the reader checks (AC23)
A promotion pull request passes `checks/promotion` only when:
- it changes nothing in `environments/rollouts.yaml` but appended entries, each valid as above;
- the three digests it sets in `environments/production/values.yaml` (web, API, worker) share one revision, and that
  revision has `result: completed` entries for all three in `environment: staging`, in the file as it stands after the
  pull request; or the three are together the production set of an earlier promotion (on `main` before this pull
  request) (spec v3, P4);
- it names the failing digests and their revisions otherwise.

The records are the person's statement, not facts from the cluster (KL3). The check reports; on GitHub Free it blocks
nothing (KL1).

## Tests (drafts)
- Reader, `jobwright-gitops`: [tests/rollout-records/test_promotion_check.py](tests/rollout-records/test_promotion_check.py)
