<!-- aidlc template: intent -->
# NEW-jobwright · Jobwright version 1 · intent

## For the reviewer
- **Asked of:** The product owner, to accept or decline the work, set its priority and agree the provisional tier with whoever owns the risk
- **Record it:** In the tracker (not yet chosen: requirements Q1); until then, in this file's pull request
- **Tier and main risk:** 3: one service company seeing or changing another's data, or customers' and technicians' personal data leaking
- **Changed since the last version:** first version

This is a check of the requirements, not a redraft. The source,
[docs/requirements/2026-11-08-jobwright-requirements.md](../../docs/requirements/2026-11-08-jobwright-requirements.md)
(v1.0), already states the problem, observable goals and testable criteria. The statement below only summarises it with
section references; the source stays the story. The questions after it are what is missing or unclear.

```text
Kind      new system (route G)
Problem   Companies that maintain equipment at customer sites plan their technicians' day in spreadsheets and
          phone calls. Dispatchers cannot see which service promises are about to be missed; technicians phone the
          office to report progress; customers phone to ask when someone will arrive (section 1).
          Why now: the product is the system aidlc is demonstrated on at a public event, and it must be production
          grade before then (section 1, G4). Evidence: the owner's design sessions of 2026-11-06; no support
          reports or incidents exist, because there is no product yet.
Outcome   - A dispatcher answers "what is late or about to be?" within 10 seconds of signing in (G1).
          - A job goes from scheduled to completed with no phone call, also where the technician has no signal (G2).
          - A customer contact follows their job from a link, with no account (G3).
          - Each service company sees and changes only its own data (FR-IAM-02).
          - Every quality gate in section 8 passes on every change, and a bad release reverses itself with no
            person acting (G4).
Not now   - The live dispatch board (section 7): a later story the owner starts; version 1 contains none of it.
          - Everything section 4 lists as not in scope (invoicing, inventory, self-booking, route optimisation,
            payroll, native apps, other languages and currencies).
          - The order of the increments (section 4) is Proposed; aidlc-design sets the slices.
          Constraints: section 9 is Decided by the owner; aidlc-specify and aidlc-design build within it and do
          not reopen it.
Owner     Prashant Sharma (product owner, tech lead and approver, assumption 11).
          Consulted: security and privacy (who reviews: question I3). Provisional tier: 3
          (dials: risk high, complexity high, scope high; when uncertain, go up a tier)
```

Why Tier 3: risk is high, because the product holds several companies' data in one deployment, signs people in, and
holds customer contacts' personal data and a technician's live position. Complexity is high: offline changes that
arrive exactly once, live updates across two running versions, and a release that rolls itself back. Scope is high:
two repositories and four increments, though scope alone would not make it Tier 3.

Gate: an owner and a priority are set, the outcome is accepted, and the statement names no implementation.

## Questions for the product owner before accepting

| ID | Question | Why it matters |
|---|---|---|
| I1 | **Where does this story end?** (a) One story for all of version 1, increments 1 to 4, released once at the end. (b) This story is the new system and increment 1 (Foundations), released by canary; increments 2, 3 and 4 are each a later story. (c) This story is the new system and increments 1 and 2 (sign-in through the office), with field, customer and hardening as later stories. | Route G builds the system and then "the story's criteria"; later features are new stories. With (a), one release comes only at the end, and the coordination plan carries every increment. |
| I2 | **Priority, the event date and the tracker** (requirements Q1). | No date is given for "before the event". The key stays `NEW-jobwright` until the item exists. |
| I3 | **Who does the Tier 3 specialist reviews** (security, privacy, release)? Assumption 11 makes you the only person. | Tier 3 needs a recorded specialist review. If the reviewer is you, that is recorded as a known limit, not skipped. |
| I4 | **Is technicians' data personal data too?** FR-PRV-01 names only customer contacts. A technician's live position (FR-CST-03), home base location (FR-TEC-01) and phone are personal data as well; they have no retention rule and are not covered by export or erasure. | It sets the privacy scope of the spec, and it may add criteria. |

## Already found, for aidlc-specify (no answer needed to accept the intent)

| ID | Finding | Source |
|---|---|---|
| S1 | Erasure replaces a contact's personal fields, but audit records "cannot be edited or deleted by anyone through the product" and keep before and after values, which may hold the same personal data. Which one wins for audit records? | FR-PRV-02, FR-AUD-01 |
| S2 | Live updates must appear within 2 seconds (95th percentile) on the technician's Today and the customer page, but the cancellation reaches the technician "within 5 seconds", and "On the way" reaches the customer page "within 5 seconds". | FR-LIVE-01, FR-JOB E1, FR-CST-03 AC3 |
| S3 | First-time fix counts "no follow-up job created from them", but nothing lets a job be created from, or linked to, another job. | FR-OVR-01, FR-JOB-01 |
| S4 | "The tenant's single sign-on": does each company bring its own identity provider, or are accounts held by Jobwright and invited by email (FR-ADM-01)? And who creates a tenant? No role or command does, except demo data. | FR-IAM-01, FR-ADM-01, NFR-PERF-04 |
| S5 | The formatting gate is required in CI, while whether web formatting is enforced is still open. | section 8, Q4 |
| S6 | A pause moves respond-by; does it also move fix-by, and can a clock be paused after the response is met? | FR-SLA-03 |
| S7 | The site contact's email is not required, but the completion email goes to the site contact. What happens when there is none? | FR-CUS AC2, FR-NTF-01 |
| S8 | No role is given for Reports in the permissions table. | section 2, FR-SIMPLE-02 |
| S9 | "Apex Facility Services" and "Brightline Services" must be invented, but they may match real businesses; someone should check before they reach the demo. | FR-DEMO-01 to 03 |
| S10 | Every Proposed value (requirements Q2) and the greeting's time zone (Q3). | Q2, Q3 |

For aidlc-design, not a question: the platform list in section 9 (cluster, database, identity, storage, mail, flags,
deployment, canary analysis, metrics) has to fit in about 6 GB (section 9.5). That is a feasibility risk to check
early.
