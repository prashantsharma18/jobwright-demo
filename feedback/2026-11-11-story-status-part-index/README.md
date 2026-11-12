# Feedback · 2026-11-11 · the story status command cannot read the part indexes

```text
FOUND IN   NEW-jobwright, after part 1 (jobwright-app slice 0) merged through pull request #1 (d612731)
FOUND BY   python3 scripts/story_status.py NEW-jobwright --write, run by Prashant Sharma, 2026-11-11
OWNER      Prashant Sharma (owner of the workspace root, both repositories and the aidlc set-up)
EFFECT     status.md shows part 1 as "waiting" although it is merged; --check-release can never pass for this story
STATUS     decided 2026-11-11: A for this story, B raised with aidlc (Claude at Prashant Sharma's instruction;
           stories/NEW-jobwright/decisions-2026-11-11.md, decision 4)
           part 1: fixed by jobwright-app pull request #4 (bc81899, 2026-11-12); part 2's index already uses fix A's layout
           (draft on feat/NEW-jobwright-slice-0); B still open with aidlc, owned by Prashant Sharma
```

## What happened

`stories/NEW-jobwright/status.md` (generated 2026-11-11):

| Part | Repository | Reached | Needs |
|---|---|---|---|
| 1 | jobwright-app | waiting | no stage rows recognised in the part index; PINS item not understood: contract none |
| 2 | jobwright-gitops | waiting | no stage rows recognised in the part index; no PINS line |

Part 1 is merged (jobwright-app `plans/NEW-jobwright/part-1/README.md`, row "slice 0": "... merged by Prashant
Sharma; next: aidlc-onboard (confirm)"). Part 2 has not started, so its "waiting" is correct, but the warnings come
from the same cause.

## Why

Two formats that are both aidlc's do not agree.

1. **Stage rows.** `scripts/story_status.py` (line 24) reads only rows named `aidlc-plan`, `aidlc-build`,
   `aidlc-verify`, `aidlc-review` and `merged`, and a part counts as merged only when its `merged` row reads `done`.
   The part indexes that aidlc-onboard (prepare) wrote in both repositories have three rows instead:
   `aidlc-onboard (prepare)`, `slice 0` and `aidlc-onboard (confirm)`. The whole history of slice 0, from plan to
   merge, sits in the one `slice 0` cell. No row matches, so every part stays at "waiting".
2. **PINS.** The script reads each PINS item as `<name> v<n>`, and it skips an item that is exactly `none`. Part 1's
   PINS line reads `spec v3 · plan v4 · contract none`: slice 0 really pins no contract, but `contract none` matches
   neither form.

## Effect

- `status.md` misreports a merged part, so the workspace root cannot see the story's real position from it.
- `--check-release` needs every part `merged` with no needs (`release_ready`). With these indexes it fails for the whole
  story even when all four parts are merged, so the release gate cannot be trusted or must be bypassed.
- Parts 3 and 4 will hit the same problem if their indexes copy the slice 0 layout.

## Options

**A. Change the part indexes to the script's format** (each repository's own session, in its own pull request):
- Give each part index the stage rows `aidlc-plan`, `aidlc-build`, `aidlc-verify`, `aidlc-review` and `merged`,
  keeping `aidlc-onboard (prepare)` and `aidlc-onboard (confirm)` beside them (the script ignores rows it does not
  know). Part 1's `merged` row reads `done (pull request #1, d612731)`; its long `slice 0` cell moves to a log
  or stays as history.
- Drop `contract none` from part 1's PINS (`spec v3 · plan v4`).
- A part index that changes reaches the default branch only through a merge (story README), so part 1 needs a small
  pull request in jobwright-app.

**B. Change the script to read the layout aidlc actually writes** (the owner, or the aidlc feedback route; this
workspace's sessions may not edit `scripts/`):
- Treat a `slice <n>` row whose status contains "merged" (or a final `done`) as `merged`, or read the stage names
  inside it.
- Accept `<name> none` in PINS as "pins nothing for <name>".
- Add tests for both, using the two real part indexes as fixtures.

**Recommendation:** A for this story, because the next slice's index is being written now anyway (part 2's aidlc-plan,
part 1's onboard confirm) and A needs no tool change. Also raise B with aidlc, so that the onboard (prepare) template
and the status command agree for the next story.

## Until it is fixed

- Do not commit `status.md` as a record of part 1's state; the story index (`stories/NEW-jobwright/README.md`) and
  each part index are the source of truth.
- Re-run `python3 scripts/story_status.py NEW-jobwright --write` after the fix: part 1 should read `merged` with no needs.
