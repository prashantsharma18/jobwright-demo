"""Draft reader tests for the Release ledger v2, the rollout records (NEW-jobwright amendment 1; AC23 as spec v3
words it, P4: the three images share one revision that completed a staging rollout together). Owner: Prashant Sharma.

Destination: jobwright-gitops, checks/test_promotion.py (Python 3 standard library, unittest). The check under test
is checks/promotion.py, given the records and production sets on main before the pull request and the records and
production set after it. They run red in the first step of their slice.
"""
import unittest

from promotion import check_promotion  # checks/promotion.py

R1, R2 = "1" * 40, "2" * 40
WORKLOADS = ("web", "api", "worker")


def d(rev: str, workload: str) -> str:
    return "sha256:" + (rev[:8] + {"web": "a", "api": "b", "worker": "c"}[workload] * 56)


def digests(rev: str) -> dict:
    return {w: d(rev, w) for w in WORKLOADS}


def records(environment: str, rev: str, result: str = "completed", only=WORKLOADS) -> list:
    return [{"environment": environment, "workload": w, "digest": d(rev, w), "revision": rev, "result": result,
             "at": "2026-11-20T10:00:00Z", "seen_in": f"argo rollouts get rollout jobwright-{w}"} for w in only]


class PromotionCheckTests(unittest.TestCase):
    def test_AC23_one_revision_completed_in_staging_for_all_three_passes(self):
        outcome = check_promotion(records_before=[], production_before=[],
                                  records_after=records("staging", R2), production_after=digests(R2))

        self.assertTrue(outcome.passed, outcome.message)

    def test_AC23_P4_a_mixed_set_fails_even_when_each_digest_completed_in_staging(self):
        mixed = {**digests(R2), "api": d(R1, "api")}
        recs = records("staging", R1) + records("staging", R2)

        outcome = check_promotion(records_before=[], production_before=[], records_after=recs, production_after=mixed)

        self.assertFalse(outcome.passed)
        self.assertIn(R1, outcome.message)
        self.assertIn(R2, outcome.message)

    def test_AC23_P4_a_revision_completed_for_only_two_of_three_fails(self):
        recs = records("staging", R2, only=("web", "api")) + records("staging", R2, result="aborted", only=("worker",))

        outcome = check_promotion(records_before=[], production_before=[], records_after=recs, production_after=digests(R2))

        self.assertFalse(outcome.passed)
        self.assertIn(d(R2, "worker"), outcome.message)

    def test_AC23_a_rollback_to_an_earlier_production_set_passes(self):
        before = records("production", R1)

        outcome = check_promotion(records_before=before, production_before=[digests(R1)],
                                  records_after=before, production_after=digests(R1))

        self.assertTrue(outcome.passed, outcome.message)

    def test_AC23_a_production_record_added_in_this_same_pull_request_does_not_count(self):
        outcome = check_promotion(records_before=[], production_before=[],
                                  records_after=records("production", R1), production_after=digests(R1))

        self.assertFalse(outcome.passed)

    def test_rollout_records_v2_existing_entries_cannot_change(self):
        before = records("staging", R1)
        tampered = records("staging", R1, result="aborted")

        outcome = check_promotion(records_before=before, production_before=[],
                                  records_after=tampered, production_after=digests(R1))

        self.assertFalse(outcome.passed)
        self.assertIn("only grows", outcome.message)

    def test_rollout_records_v2_an_entry_with_a_malformed_digest_is_refused(self):
        bad = records("staging", R2)
        bad[0]["digest"] = "sha256:short"

        outcome = check_promotion(records_before=[], production_before=[], records_after=bad, production_after=digests(R2))

        self.assertFalse(outcome.passed)


if __name__ == "__main__":
    unittest.main()
