<!-- aidlc template: intent -->
# NEW-jobwright · Jobwright: the new system and its foundations · intent

Version 2 · 2026-11-08

## For the reviewer
- **Asked of:** The product owner, to accept or decline the work, set its priority and agree the provisional tier with whoever owns the risk
- **Record it:** The tracker is files in this workspace for now (answer I2), so the acceptance is this file merged to `main` by the product owner
- **Tier and main risk:** 3: one service company seeing or changing another's data, or a weakness in signing people in
- **Changed since the last version:** the product owner answered I1 to I4 on 2026-11-08. The story now covers the new system and increment 1 only; the outcome and "Not now" changed to match; technicians' data is recorded as personal data; findings are sorted into this story and the later ones.

This is a check of the requirements, not a redraft. The source,
[docs/requirements/2026-11-08-jobwright-requirements.md](../../docs/requirements/2026-11-08-jobwright-requirements.md)
(v1.0), remains the story; the statement below summarises it with section references.

```text
Kind      new system (route G)
Problem   Companies that maintain equipment at customer sites plan their technicians' day in spreadsheets and
          phone calls. Dispatchers cannot see which service promises are about to be missed; technicians phone the
          office to report progress; customers phone to ask when someone will arrive (section 1).
          This story lays the foundation every later capability needs: people of several companies signing in to
          one product that keeps each company apart, and a way to release it safely.
          Why now: the product is the system aidlc is demonstrated on at a public event, and it must be production
          grade before then (section 1, G4). Evidence: the owner's design sessions of 2026-11-06; no support
          reports or incidents exist, because there is no product yet.
Outcome   Increment 1, Foundations (section 4: FR-IAM, FR-SHELL, the qualities of section 6, section 9):
          - A dispatcher, technician or tenant admin signs in with their company's sign-in, returns to the page
            they asked for, and can sign out (FR-IAM-01).
          - A person in one company never sees, changes or learns about another company's data, even where the
            product's own code forgets to keep them apart (FR-IAM-02).
          - Each person is offered only what their role may do (FR-IAM-03, FR-SHELL-01 AC3).
          - Every page has the navigation, light and dark, and the company and user in the top bar, on desktop and
            phone (FR-SHELL-01 to 03, section 9.3).
          - Every quality gate in section 8 that applies to this increment passes on every change.
          - A new version reaches users in steps and reverses itself when it misbehaves, with no person acting
            (NFR-REL-02; how much of the proof belongs here: S11).
Not now   - Increment 2 (Office), increment 3 (Field and customer) and increment 4 (Hardening): each is a later
            story (answer I1). Goals G1 to G3 are reached in those stories.
          - The live dispatch board (section 7): a later story the owner starts; nothing of it is built before.
          - Everything section 4 lists as not in scope.
          Constraints: section 9 is Decided by the owner; aidlc-specify and aidlc-design build within it and do
          not reopen it.
Owner     Prashant Sharma (product owner, tech lead and approver, assumption 11). Priority: high (answer I2).
          Consulted: security, privacy and release, all by Prashant Sharma (answer I3). Provisional tier: 3
          (dials: risk high, complexity high, scope medium; when uncertain, go up a tier)
```

Why Tier 3: risk is high because this increment signs people in and keeps several companies' data apart in one
deployment. Complexity is high: a whole platform, a release that rolls itself back, and two repositories built from
nothing. Scope is medium now that the story is one increment.

Gate: an owner and a priority are set, the outcome is accepted, and the statement names no implementation.

## The product owner's answers (2026-11-08)

| ID | Answer | What it changes |
|---|---|---|
| I1 | (b): this story is the new system plus increment 1; increments 2, 3 and 4 become later stories. | The outcome and "Not now" above. |
| I2 | Priority high. Tracker: files in this workspace for now, the lab Jira possibly later. Event date: not given (the answer held a placeholder). | The key stays `NEW-jobwright` until the product owner names it. The event date is still open. |
| I3 | Prashant Sharma does the security, privacy and release reviews. | **Known limit:** at Tier 3 the specialist reviews are done by the same person who owns the product, leads and approves. No independent specialist checks this work; each review is recorded, not skipped. |
| I4 | A technician's position, home base and phone are personal data, and are added to retention, export and erasure. | The privacy requirement (FR-PRV) is built in increment 3, so this binds that later story. Until the requirements carry it (proposal below), this file is its record. |

Still open, for the product owner: the event date, and the story's key if it should differ from `NEW-jobwright`.

**Proposal for the requirements' owner:** a version 1.1 of the requirements that adds answer I4 to FR-PRV-01 and moves
the findings for the later stories (below) into section 12, so each later story starts from them. This file does not
change the requirements.

## Findings for this story, for aidlc-specify

| ID | Finding | Source |
|---|---|---|
| S4 | "The tenant's single sign-on": does each company bring its own identity provider, or are accounts held by Jobwright and invited by email (FR-ADM-01)? And who creates a tenant? No role or command does, except demo data. | FR-IAM-01, FR-ADM-01, NFR-PERF-04 |
| S5 | The formatting gate is required in CI, while whether web formatting is enforced is still open. | section 8, Q4 |
| S10 | The Proposed session times (60 minutes idle, 12 hours absolute) and the Proposed edge cases E1 and E2 of FR-IAM-03. | Q2, FR-IAM |
| S11 | Section 4 lists "NFR" in increment 1 and again in increment 4, which holds "a proven canary rollback" and the full browser tests, budgets and dashboards. Which qualities and gates does this story prove, and which wait for increment 4? | section 4, section 8, NFR-REL-03 |
| S12 | The isolation criteria name jobs (FR-IAM-02 AC1, AC2), which arrive in increment 2. What proves isolation in this story? | FR-IAM-02 |

## Findings for the later stories (from version 1)

| ID | Finding | Story | Source |
|---|---|---|---|
| S1 | Erasure replaces a contact's personal fields, but audit records "cannot be edited or deleted by anyone through the product" and keep before and after values, which may hold the same personal data. With I4, a technician's data is in the same position. | increments 2 and 3 | FR-PRV-02, FR-AUD-01 |
| S2 | Live updates within 2 seconds (95th percentile) on Today and the customer page, against "within 5 seconds" for the cancellation and for "On the way". | increment 3 | FR-LIVE-01, FR-JOB E1, FR-CST-03 AC3 |
| S3 | First-time fix counts "no follow-up job created from them", but nothing links a job to another job. | increment 2 | FR-OVR-01, FR-JOB-01 |
| S6 | Does a pause also move fix-by, and can a clock be paused after the response is met? | increment 2 | FR-SLA-03 |
| S7 | The site contact's email is not required, but the completion email goes to them. | increment 3 | FR-CUS AC2, FR-NTF-01 |
| S8 | No role is given for Reports in the permissions table. | increment 2 | section 2, FR-SIMPLE-02 |
| S9 | "Apex Facility Services" and "Brightline Services" must be invented, but may match real businesses; check before the demo. | increment 4 | FR-DEMO-01 to 03 |

For aidlc-design, not a question: the platform list in section 9 has to fit in about 6 GB (section 9.5). That is a
feasibility risk to check early.
