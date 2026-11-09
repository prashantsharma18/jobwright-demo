# Contract · Released versions · v2 (draft, NEW-jobwright amendment 1)

Owner: Prashant Sharma. Provider: `jobwright-app` CI (it publishes the images and commits staging's versions).
Consumer: `jobwright-gitops` (its CI on each staging commit, its promotion check, and Argo CD). Both ends have the same
owner, who approves this version as provider and consumer.

**Change from v1: major (breaking), done by replacement before any release.** v1 named a local registry and a local
Git server; v2 names GitHub's registry and repository, adds immutable `sha-` tags, revision and run-number labels, and
a guard against going backwards. Under spec v3 (P1) the provider still commits staging's versions, with the one stored
token, right after publishing. Nothing implements v1 yet (no slice has started), so there is no expand and contract:
v1 is withdrawn and v2 replaces it.

## What the provider publishes
On every push to `main` of `jobwright-app`, after every gate has passed in that run:

| Image | Name |
|---|---|
| web | `ghcr.io/prashantsharma18/jobwright-web` |
| API | `ghcr.io/prashantsharma18/jobwright-api` |
| worker | `ghcr.io/prashantsharma18/jobwright-worker` |

- Each image is built once in that run and pushed by digest, with the tag `sha-<the 40-character commit>`.
- Each carries the labels `org.opencontainers.image.revision=<commit>`,
  `org.opencontainers.image.source=https://github.com/prashantsharma18/jobwright-app` (so the package is linked to the
  repository) and `dev.jobwright.run-number=<the workflow run number>`.
- The main-branch workflow is one `concurrency` group: runs never overlap, and a newer pending run replaces an older
  one.
- Only after all three are pushed, and the browser tests have passed against those digests, does the provider move the
  tag `main` on each (web, then API, then worker) and then commit staging's versions (below).
- A tag `sha-*` is never moved or reused. `main` is the only moving tag.

## The staging commit (provider, with the staging token)
Right after moving the `main` tags, the same run writes `environments/staging/values.yaml` in `jobwright-gitops`:

```yaml
revision: <40-hex commit>
runNumber: <the run number>
images:
  web: ghcr.io/prashantsharma18/jobwright-web@sha256:<64 hex>
  api: ghcr.io/prashantsharma18/jobwright-api@sha256:<64 hex>
  worker: ghcr.io/prashantsharma18/jobwright-worker@sha256:<64 hex>
```

and commits it to `main` with the message `staging: <revision>`. It changes no other file. It refuses, and fails the
run visibly, when the file already holds a higher `runNumber` (never backwards). A push rejected because `main` moved is
rebased and retried once.

## What the consumer may rely on
- Environments name digests, never tags; the three digests in a staging commit share one revision.
- `runNumber` in staging never decreases.
- A commit made with the staging token starts `jobwright-gitops`'s workflows, so its CI renders and validates every
  staging commit (it cannot block it on Free: KL1).
- No image is deleted automatically while the registry is free (spec v3, E30).

## Tests (drafts; they run red in the first step of their slice)
- Provider, `jobwright-app`: [tests/released-versions/test_published_images.py](tests/released-versions/test_published_images.py)
- Provider, `jobwright-app` (the staging commit): [tests/released-versions/test_staging_commit.py](tests/released-versions/test_staging_commit.py)
- Consumer, `jobwright-gitops`: its existing render and values checks run on every staging commit (the values check
  refuses a tag, a mixed revision or a lower run number)
