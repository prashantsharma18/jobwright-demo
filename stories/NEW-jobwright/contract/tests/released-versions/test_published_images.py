"""Draft provider tests for Released versions v2 (NEW-jobwright amendment 1). Owner: Prashant Sharma.

Destination: jobwright-app, scripts/test_publish_plan.py (Python 3 standard library, unittest). The publish step of
the main-branch workflow is driven by scripts/publish_plan.py, which turns the run's commit and the three built
digests into the ordered list of registry operations; the workflow executes that list. Testing the plan keeps the
contract's ordering rules testable without a registry. They run red in the first step of their slice.
"""
import unittest

from publish_plan import plan_publish  # scripts/publish_plan.py

COMMIT = "c" * 40
DIGESTS = {"web": "sha256:" + "1" * 64, "api": "sha256:" + "2" * 64, "worker": "sha256:" + "3" * 64}


class PublishPlanTests(unittest.TestCase):
    def setUp(self):
        self.plan = plan_publish(commit=COMMIT, digests=DIGESTS, owner="prashantsharma18")

    def test_released_versions_v2_every_image_gets_an_immutable_sha_tag(self):
        pushes = [op for op in self.plan if op.kind == "push"]

        self.assertEqual(
            {f"ghcr.io/prashantsharma18/jobwright-{w}:sha-{COMMIT}" for w in DIGESTS},
            {op.reference for op in pushes})

    def test_released_versions_v2_main_moves_only_after_all_three_are_pushed(self):
        kinds = [op.kind for op in self.plan]

        last_push = max(i for i, k in enumerate(kinds) if k == "push")
        first_move = min(i for i, k in enumerate(kinds) if k == "move-main")
        self.assertLess(last_push, first_move)

    def test_released_versions_v2_main_moves_web_then_api_then_worker(self):
        moves = [op.workload for op in self.plan if op.kind == "move-main"]

        self.assertEqual(["web", "api", "worker"], moves)

    def test_released_versions_v2_every_image_carries_revision_and_source_labels(self):
        for op in (op for op in self.plan if op.kind == "push"):
            self.assertEqual(COMMIT, op.labels["org.opencontainers.image.revision"])
            self.assertEqual("https://github.com/prashantsharma18/jobwright-app",
                             op.labels["org.opencontainers.image.source"])

    def test_released_versions_v2_a_malformed_commit_is_refused(self):
        with self.assertRaises(ValueError):
            plan_publish(commit="main", digests=DIGESTS, owner="prashantsharma18")


if __name__ == "__main__":
    unittest.main()
