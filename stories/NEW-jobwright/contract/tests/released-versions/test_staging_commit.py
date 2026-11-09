"""Draft provider tests for the staging commit of Released versions v2 (NEW-jobwright amendment 1, spec v3 P1).
Owner: Prashant Sharma.

Destination: jobwright-app, scripts/test_staging_commit.py (Python 3 standard library, unittest). The step under
test is scripts/staging_commit.py: given the current `environments/staging/values.yaml` of jobwright-gitops and this
run's commit, run number and digests, it returns the new file content, or refuses. Git and GitHub are the replaced
edges; the step only computes. They run red in the first step of their slice.
"""
import unittest

from staging_commit import StagingRefused, next_staging  # scripts/staging_commit.py

OLD, NEW = "a" * 40, "b" * 40
DIGESTS = {"web": "sha256:" + "1" * 64, "api": "sha256:" + "2" * 64, "worker": "sha256:" + "3" * 64}
CURRENT = {
    "revision": OLD, "runNumber": 41,
    "images": {w: f"ghcr.io/prashantsharma18/jobwright-{w}@sha256:" + "9" * 64 for w in ("web", "api", "worker")},
}


class StagingCommitTests(unittest.TestCase):
    def test_released_versions_v2_a_newer_run_writes_all_three_digests_and_the_revision(self):
        result = next_staging(CURRENT, revision=NEW, run_number=42, digests=DIGESTS, owner="prashantsharma18")

        self.assertEqual(NEW, result.values["revision"])
        self.assertEqual(42, result.values["runNumber"])
        self.assertEqual(f"ghcr.io/prashantsharma18/jobwright-api@{DIGESTS['api']}", result.values["images"]["api"])
        self.assertEqual(f"staging: {NEW}", result.message)

    def test_released_versions_v2_an_older_run_is_refused_so_staging_never_goes_backwards(self):
        with self.assertRaises(StagingRefused) as refused:
            next_staging(CURRENT, revision=NEW, run_number=40, digests=DIGESTS, owner="prashantsharma18")

        self.assertIn("41", str(refused.exception))

    def test_released_versions_v2_the_same_run_again_changes_nothing(self):
        same = {**CURRENT, "revision": NEW, "runNumber": 42,
                "images": {w: f"ghcr.io/prashantsharma18/jobwright-{w}@{d}" for w, d in DIGESTS.items()}}

        result = next_staging(same, revision=NEW, run_number=42, digests=DIGESTS, owner="prashantsharma18")

        self.assertIsNone(result.message)

    def test_released_versions_v2_a_missing_or_tag_named_image_is_refused(self):
        for bad in ({"web": DIGESTS["web"], "api": DIGESTS["api"]}, {**DIGESTS, "worker": "main"}):
            with self.subTest(digests=bad), self.assertRaises(StagingRefused):
                next_staging(CURRENT, revision=NEW, run_number=42, digests=bad, owner="prashantsharma18")

    def test_released_versions_v2_only_the_staging_values_file_is_written(self):
        result = next_staging(CURRENT, revision=NEW, run_number=42, digests=DIGESTS, owner="prashantsharma18")

        self.assertEqual("environments/staging/values.yaml", result.path)


if __name__ == "__main__":
    unittest.main()
