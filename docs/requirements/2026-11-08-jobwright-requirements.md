# Jobwright: requirements

Version 1.0 · 2026-11-08 · Product owner: Prashant Sharma

This document says what Jobwright must do and how well. It does not say how to build it, except where the owner has
already decided (section 9, Constraints). Hand it to aidlc as a new system: `aidlc-start` routes it, `aidlc-intent`
checks it, and `aidlc-specify` turns each capability into a specification.

How to read the labels:
- **Decided** means the owner chose it during the design sessions of 2026-11-06. Change it only with the owner.
- **Proposed** means this document suggests it. The spec owner confirms or changes it in `aidlc-specify`.
- Every requirement has an ID (for example `FR-JOB-03`). Specs, tests and pull requests cite these IDs.

---

## 1. The problem

Companies that maintain equipment (air conditioning, lifts, generators, chillers, fire panels) send technicians to
customer sites. Today their dispatchers plan the day in spreadsheets and phone calls. Nobody sees which service
promises (SLAs) are about to be missed. Technicians call the office to report progress. Customers call to ask when
someone will arrive.

Jobwright is a field-service management product for these companies. One deployment serves many companies; each is a
**tenant** and sees only its own data.

### Goals
| ID | Goal | How we know |
|---|---|---|
| G1 | Dispatchers see every open job and every SLA at risk in one place | A dispatcher answers "what is late or about to be?" in under 10 seconds from sign-in |
| G2 | Technicians report progress from the site, even without a signal | No phone call is needed to move a job from scheduled to completed |
| G3 | Customers know when help arrives without calling | The customer follows the job from a link, with no account |
| G4 | The product is production grade, not a prototype | Every quality gate in section 8 passes in CI, and a bad release rolls itself back |

### What this document is for
Jobwright is the system aidlc is demonstrated on, at a public event, in a 2-minute video. Everything here is built
before the event except one capability, the **live dispatch board** (section 7), which is built on camera.

---

## 2. Who uses it

| Role | Device | What they do | Decided |
|---|---|---|---|
| Dispatcher | Desktop browser | Takes calls, creates jobs, assigns technicians and time windows, watches SLAs | Decided |
| Technician | Phone (installable web app) | Sees today's jobs, travels, works the checklist, records photos, parts and the customer's signature | Decided |
| Customer contact | Phone, from a text message link | Sees the arrival window, the technician and progress; no account, no sign-in | Decided |
| Tenant admin | Desktop browser | Manages users and roles, SLA policies and the tenant's settings | Decided |

### Permissions
| Action | Dispatcher | Technician | Tenant admin | Customer (link) |
|---|---|---|---|---|
| See all the tenant's jobs | yes | no, only jobs assigned to them | yes | no, only the linked job |
| Create, edit, assign, cancel jobs | yes | no | yes | no |
| Move a job en route, on site, completed | yes | own jobs only | yes | no |
| Manage customers, sites, assets | yes | no (read the job's site and asset) | yes | no |
| Manage users, roles, SLA policies, tenant settings | no | no | yes | no |
| Pause or resume an SLA clock | yes | no | yes | no |
| Export or erase a customer's personal data | no | no | yes | no |

A user has exactly one role in a tenant. A person may belong to more than one tenant (Proposed).

---

## 3. Words used in this document

| Term | Meaning |
|---|---|
| Tenant | A service company using Jobwright. All its data is invisible to every other tenant. |
| Customer | An organisation the tenant serves (a hospital, a mall, an office park). |
| Site | A customer's address where work happens. A customer has one or more sites. |
| Asset | A piece of equipment at a site: make, model, serial number, install date. |
| Job | One piece of work at one site, usually on one asset. It has a number (`JB-2041`), a type, a priority and a status. |
| Assignment | A technician and a planned time window for a job. |
| SLA policy | The tenant's promise per priority: a response target and a fix target. |
| SLA clock | A job's two due times (respond by, fix by), its pauses and whether it is breached. |
| Response | The moment a technician arrives on site (the job enters On site). |
| Fix | The moment the job is completed. |
| Visit | One period on site, from On site to the next status change. |

---

## 4. Scope

### In scope (version 1, built before the event)
Every capability in section 5, with the qualities in section 6, the look in section 9.3 and the delivery in section 9.4.

### Built on camera (not in version 1)
The live dispatch board, section 7. Version 1 must not contain it, not even switched off.

### Not in scope
Invoicing and payments · inventory and stock control · customer self-booking · route optimisation · technician
payroll and timesheets · native mobile apps (the technician app is an installable web app) · languages other than
English · multiple currencies.

### Increments (Proposed order; each ends usable and demonstrable)
| Increment | Delivers | Requirements |
|---|---|---|
| 1 · Foundations | Sign-in, the app shell, the design system, the platform, delivery by canary | FR-IAM, FR-SHELL, NFR, section 9 |
| 2 · Office | Customers, sites, assets; jobs; technicians; SLA policies and clocks; activity; users and roles; Overview with live figures | FR-CUS, FR-JOB, FR-TEC, FR-SLA, FR-OVR, FR-ADM, FR-AUD |
| 3 · Field and customer | Technician app with offline use; customer status page; notifications; live updates | FR-FLD, FR-CST, FR-NTF, FR-LIVE, FR-PRV |
| 4 · Hardening | Full browser tests, performance budgets, dashboards, alerts and runbooks, a proven canary rollback, realistic demo data, a rehearsal | NFR, FR-DEMO |
| Demo | The live dispatch board | section 7 |

---

## 5. Functional requirements

Acceptance criteria are written as Given, When, Then. Each capability ends with its edge cases and who decided them.

### 5.1 Sign-in and tenancy (FR-IAM)

**FR-IAM-01 Single sign-on.** Every user except a customer signs in through the tenant's single sign-on. (Decided)
```
AC1  Given a user with an account in tenant Apex
     When they open any page without a session
     Then they are sent to sign in, and after signing in they return to the page they asked for
AC2  Given a signed-in user
     When they choose "Sign out"
     Then their session ends on the server, and the next request asks them to sign in again
AC3  Given a user who has been idle for 60 minutes, or signed in 12 hours ago
     When they make a request
     Then they are asked to sign in again (Proposed: 60 minutes idle, 12 hours absolute)
```

**FR-IAM-02 Tenant isolation.** A user never sees, changes or learns about another tenant's data. (Decided)
```
AC1  Given jobs in tenants Apex and Brightline
     When a dispatcher of Apex lists, searches, opens or edits any record
     Then only Apex records are returned, and asking for a Brightline record by its ID answers "not found" (404), never "forbidden"
AC2  Given application code that forgets to filter by tenant
     When it reads jobs
     Then the data store still returns only the current tenant's rows (isolation is enforced below the application)
```

**FR-IAM-03 Roles.** What each role may do is the permissions table in section 2. (Decided)
```
AC1  Given a technician
     When they ask for a job not assigned to them
     Then the answer is "not found" (404)
AC2  Given a dispatcher
     When they open user management
     Then the page is refused (403) and the navigation does not offer it
```

Edge cases:
- E1 A user removed from a tenant while signed in → their next request is refused and the session ends. (Proposed)
- E2 A person in two tenants → they choose the tenant after sign-in and can switch; data never mixes. (Proposed)

### 5.2 The application shell (FR-SHELL)

**FR-SHELL-01 Navigation.** Sections: Overview, Jobs, Technicians, Customers, Assets, Reports, Settings. (Decided)
```
AC1  Given a screen 768 px wide or wider
     When any page loads
     Then a left sidebar lists every section and marks the current one
AC2  Given a screen narrower than 768 px
     When any page loads
     Then a bottom tab bar shows Overview, Jobs, Technicians and Customers, and every other section is reachable from an "All sections" control in the top bar
AC3  Given a user without access to a section (for example Settings for a dispatcher)
     When the navigation renders
     Then that section is not offered
```

**FR-SHELL-02 Light and dark.** Every page works in light and dark; the default follows the device. (Decided)
```
AC1  Given a user who switches to dark
     When they reload or come back later
     Then the choice is kept, and they can return to "follow the device"
```

**FR-SHELL-03 Tenant and user.** The top bar shows the tenant's name and the signed-in user's name and role.

### 5.3 Customers, sites and assets (FR-CUS)

**FR-CUS-01 Customers.** A dispatcher creates, edits and finds customers by name, phone or site address.
**FR-CUS-02 Sites.** A customer has one or more sites, each with an address, a map location and site contacts (name, phone, email).
**FR-CUS-03 Assets.** A site has assets with make, model, serial number and install date. The Assets section lists them with filters by customer, site and make.
```
AC1  Given a new customer with a name and one site with an address
     When the dispatcher saves it
     Then it appears in search results within 2 seconds
AC2  Given a site without a postcode or a phone number for its contact
     When the dispatcher saves it
     Then the save is refused, naming each missing field
AC3  Given an asset serial number already used at the same customer
     When the dispatcher saves another asset with it
     Then the save is refused: "This serial number is already registered at this customer"
```
Edge cases:
- E1 A site's address cannot be placed on a map → it is saved, marked "location unknown", and travel estimates are not shown for it. (Proposed)
- E2 A customer with open jobs is deleted → refused; customers are archived instead, and archived customers keep their history. (Proposed)

### 5.4 Jobs (FR-JOB)

**FR-JOB-01 Create a job.** A dispatcher creates a job with customer, site, asset (optional), type (installation, repair, maintenance, inspection), priority (urgent, high, normal), description and how it was reported (phone, email, portal). The job gets the next number in the tenant (`JB-` and a number).
```
AC1  Given all required fields
     When the dispatcher creates the job
     Then it gets the next number, status New, and its SLA clock starts from the creation time
AC2  Given a missing customer, site, type or priority
     When the dispatcher creates the job
     Then it is refused, naming each missing field, and nothing is saved
AC3  Given two dispatchers creating jobs at the same moment
     When both save
     Then the two jobs get different numbers
```

**FR-JOB-02 Status.** A job's status moves only along these paths. (Decided)
```
New -> Scheduled -> En route -> On site -> Completed
New, Scheduled, En route -> Cancelled (with a reason)
Scheduled -> New (when the assignment is removed)
On site -> En route (the technician leaves to fetch parts and returns; this starts a second visit)
```
```
AC1  Given a job in Completed or Cancelled
     When anyone tries to change its status
     Then the change is refused: "This job is closed"
AC2  Given a job in New
     When someone tries to move it to On site
     Then the change is refused, naming the allowed next statuses
```

**FR-JOB-03 Jobs list.** All the tenant's jobs, with filters (status, priority, technician, customer, date, SLA state), sorting, search by number or customer, and saved views per user.
```
AC1  Given 5,000 jobs in the tenant
     When the dispatcher filters by "SLA at risk"
     Then the first page of results appears within 1 second
AC2  Given a saved view "Urgent, unassigned"
     When the dispatcher opens it the next day
     Then it shows the jobs that match now, not the ones that matched when it was saved
```

**FR-JOB-04 Job detail.** Customer, site and contacts, asset, description, SLA countdown, technician and window, activity timeline, notes, photos, parts and signature.

**FR-JOB-05 No lost updates.** Two people editing the same job never silently overwrite each other. (Decided)
```
AC1  Given two dispatchers who opened job JB-2041
     When the first saves a change and then the second saves a different change
     Then the second save is refused: "This job changed since you opened it", and the second dispatcher sees the latest version
```

**FR-JOB-06 Assign.** A dispatcher assigns a technician and a planned window (start and end). Assigning moves New to Scheduled.
```
AC1  Given a window that ends before it starts, or a technician not on shift during it
     When the dispatcher assigns
     Then the assignment is refused, naming the reason
AC2  Given a technician already assigned to another job that overlaps the window
     When the dispatcher assigns
     Then version 1 warns "This overlaps JB-…" and lets the dispatcher confirm
```
(Version 1 only warns about overlaps. Refusing them, under concurrent edits, is part of the live dispatch board, section 7.)

Edge cases:
- E1 A job is cancelled while the technician is en route → the technician's app shows "Cancelled by the office" within 5 seconds of the change when online, or at the next sync. (Proposed)
- E2 The asset on a job is archived → the job keeps it, read-only. (Proposed)

### 5.5 Technicians (FR-TEC)

**FR-TEC-01 Profile.** Skills (from a tenant list: HVAC, lifts, electrical, fire, generators, chillers), shift pattern (days and hours), home base location, phone.
**FR-TEC-02 Today's load.** The Technicians section shows each technician's jobs today, hours booked against hours on shift, and whether they are on shift now.
```
AC1  Given a technician with a shift of 09:00 to 18:00 and two assignments of 2 hours each today
     When the dispatcher opens Technicians
     Then the technician shows 4 of 9 hours booked
```

### 5.6 SLAs (FR-SLA)

**FR-SLA-01 Policies.** The tenant admin sets a response target and a fix target per priority. Proposed defaults, measured around the clock:

| Priority | Respond within | Fix within |
|---|---|---|
| Urgent | 4 hours | 8 hours |
| High | 8 hours | 24 hours |
| Normal | 24 hours | 72 hours |

**FR-SLA-02 Clock.** Each job's clock has a respond-by and a fix-by time from its creation time and policy. Response is met when the job first enters On site; fix is met when it enters Completed.
```
AC1  Given an urgent job created at 10:00
     When it is created
     Then respond-by is 14:00 and fix-by is 18:00 the same day
AC2  Given the policy changes after a job was created
     When the job's clock is shown
     Then it keeps the targets it was created with
```

**FR-SLA-03 Pause.** A dispatcher pauses a clock with a reason (customer unavailable, site closed, awaiting customer approval) and resumes it. Paused time does not count.
```
AC1  Given an urgent job paused at 11:00 and resumed at 13:00
     When its clock is shown
     Then respond-by has moved from 14:00 to 16:00
```

**FR-SLA-04 At risk and breached.** A clock is **at risk** when less than 25% of its target remains and it is not met. It is **breached** when its due time passes and it is not met. (Proposed thresholds)
```
AC1  Given an urgent job created at 10:00, not yet on site
     When the time is 13:01
     Then its response clock is at risk
AC2  When the time passes 14:00 and it is still not on site
     Then it is breached, the dispatcher sees it within 1 minute, and the breach is recorded in its activity
```

### 5.7 Overview (FR-OVR)

**FR-OVR-01 Figures.** The Overview shows four figures for the dispatcher's tenant, defined exactly:

| Figure | Definition |
|---|---|
| Open jobs | Jobs in New, Scheduled, En route or On site |
| SLA at risk | Open jobs with a clock at risk or breached |
| Technicians on shift | Technicians whose shift covers now, out of all active technicians (for example 36 / 41) |
| First-time fix | Of jobs completed in the last 30 days, the share with exactly one visit and no follow-up job created from them within 14 days |

**FR-OVR-02 Today's jobs.** Jobs planned for today, with number, customer and site, technician (or "Unassigned"), priority and status.
**FR-OVR-03 Greeting.** "Good morning", "Good afternoon" (from 12:00) or "Good evening" (from 17:00), in the tenant's time zone, with the user's first name.
```
AC1  Given a job assigned in another browser
     When the dispatcher is on the Overview
     Then the figures and the list reflect it within 2 seconds, without a reload
```

### 5.8 Technician app (FR-FLD)

**FR-FLD-01 Today.** The technician sees today's jobs in planned order, with address, window, priority and a call button for the site contact.
**FR-FLD-02 Work a job.** The technician moves the job En route, On site and Completed, works through the job's checklist, records photos (up to 10, each up to 10 MB), parts used (from a list or typed) and the customer's signature with the signer's name.
```
AC1  Given a job with a checklist of 5 required items
     When the technician tries to complete it with 4 ticked
     Then completion is refused, naming the unticked item
AC2  Given a completed job
     Then it holds the signature, the signer's name and the time of signing, and none of them can be changed afterwards
```

**FR-FLD-03 Works offline.** Everything in FR-FLD-02 works without a network. Changes wait on the phone and are sent, in order, when the network returns. (Decided)
```
AC1  Given a technician offline for 3 hours who moved a job On site, ticked items, took 4 photos and completed it
     When the network returns
     Then every change reaches the server in the order made, each exactly once, and the dispatcher sees the job Completed
AC2  Given the phone is offline
     Then the app says so on every screen, with the number of changes waiting
AC3  Given the app is closed while offline
     When it is opened again
     Then the waiting changes are still there and are sent when the network returns
AC4  Given the same queued change sent twice (a retry after a lost reply)
     Then it takes effect once
```
Edge cases:
- E1 The job was cancelled or reassigned at the office while the technician was offline → the technician's queued changes for it are refused when they arrive; the app shows "This job was changed by the office", and keeps the photos and notes so nothing is lost. The server's version always wins. (Decided: the server is authoritative)
- E2 A photo upload fails three times → it stays queued and the app shows it; the job may still be completed, marked "photos pending". (Proposed)
- E3 Offline longer than 72 hours → changes are still kept; the app asks the technician to connect. (Proposed)

**FR-FLD-04 Installable.** The app installs to the phone's home screen and opens to Today.

### 5.9 Customer status page (FR-CST)

**FR-CST-01 Link.** When a job is scheduled, the site contact gets a text message with a link. The link opens a status page with no sign-in.
**FR-CST-02 Content.** The tenant's name and logo, the job number, status (Scheduled, On the way, Arrived, Completed), the arrival window, the technician's first name, a map with the site, and a button to call the tenant.
**FR-CST-03 Location while on the way.** While the job is En route, the page shows the technician's approximate position, updated at least every 30 seconds. Position is shared only while that job is En route. (Proposed)
```
AC1  Given a link for job JB-2041
     When anyone opens it
     Then they see only JB-2041's status page; changing any part of the link shows "This link is not valid"
AC2  Given a job completed 24 hours ago, or a link created 7 days ago
     When the link is opened
     Then it shows "This link has expired" and no job details (Proposed: 24 hours after completion or 7 days after creation, whichever comes first)
AC3  Given the job moves to En route
     When the customer has the page open
     Then the page shows "On the way" within 5 seconds, without a reload
```
Edge cases:
- E1 Someone requests many links or pages from one address → rate limited (section 6.3). (Decided)
- E2 The job is cancelled → the page shows "This visit was cancelled" and the tenant's phone number. (Proposed)

### 5.10 Notifications (FR-NTF)

**FR-NTF-01 Events.** The site contact gets a text message when the job is scheduled (window and link), when the technician is on the way, and when the job is completed. An email with a summary follows completion.
**FR-NTF-02 Reliable.** A notification is sent once for each event, even when a part of the system restarts mid-way. A failed send is retried and, after 5 attempts, shown to the dispatcher on the job.
```
AC1  Given the service that sends messages restarts after the job was scheduled but before the text was sent
     When it comes back
     Then the text is sent exactly once
```
**FR-NTF-03 Stand-ins.** In every environment of this demo, email and text messages go to local stand-ins that capture them for inspection. No real message leaves the machine. (Decided)

### 5.11 Live updates (FR-LIVE)

**FR-LIVE-01** Changes made by one user appear for every other user who is looking at the same data within 2 seconds (95th percentile), with no reload: the jobs list, the job detail, the Overview, the technician's Today and the customer status page.
**FR-LIVE-02** Live updates keep working during a release, when two versions of the server run side by side.

### 5.12 Administration (FR-ADM)

**FR-ADM-01 Users.** The tenant admin invites a user by email with a role, changes a role and deactivates a user. A deactivated user cannot sign in, and their history stays.
**FR-ADM-02 SLA policies.** The admin edits the targets in FR-SLA-01. Changes apply to jobs created afterwards.
**FR-ADM-03 Tenant settings.** Name, logo (shown on the customer page), time zone, and the phone number shown to customers.

### 5.13 Activity and audit (FR-AUD)

**FR-AUD-01** Every change to a job, an assignment, an SLA clock or a user is recorded: who, what, when, the value before and the value after. Records cannot be edited or deleted by anyone through the product.
**FR-AUD-02** The job detail shows its records as an activity timeline, newest first.

### 5.14 Personal data (FR-PRV)

**FR-PRV-01** Customer contacts' names, phone numbers, emails and addresses are personal data. They never appear in logs, traces or metrics.
**FR-PRV-02** The tenant admin exports all personal data held about one contact (a file, within 1 minute) and erases it on request. Erasure replaces the personal fields with "erased" and keeps the job history.
```
AC1  Given a contact erased today
     When anyone opens a past job of theirs
     Then the job is intact, and the contact shows as "erased"
```

### 5.15 Simple sections (FR-SIMPLE)

**FR-SIMPLE-01 Assets** lists assets with filters (FR-CUS-03).
**FR-SIMPLE-02 Reports** shows SLA performance (met, breached, by priority) and first-time fix rate, by week, for the last 12 weeks.

### 5.16 Demo data (FR-DEMO)

**FR-DEMO-01** A command loads a realistic demo tenant, "Apex Facility Services" in Bengaluru (time zone Asia/Kolkata): 41 technicians, about 60 customers with sites and assets, 150 open jobs and 90 days of completed jobs, so every figure and report has believable values.
**FR-DEMO-02** Demo data uses invented organisations and people only. No real company, brand or person is named. (Proposed; the earlier mock-ups used real hospital and company names, which must not reach the product.)
**FR-DEMO-03** A second tenant, "Brightline Services", exists with a few jobs, so tenant isolation can be shown.

---

## 6. Quality requirements (NFR)

### 6.1 Performance
| ID | Requirement |
|---|---|
| NFR-PERF-01 | API reads answer within 300 ms and writes within 500 ms at the 95th percentile, at 50 requests a second, with the demo data loaded |
| NFR-PERF-02 | The Overview and the technician's Today load (largest contentful paint) within 2.5 s on a mid-range phone over a fast 4G connection |
| NFR-PERF-03 | The JavaScript sent for the first load of the Overview stays under a budget the plan sets and CI enforces |
| NFR-PERF-04 | Capacity: at least 20 tenants, 500 technicians and 5,000 open jobs in total without breaking NFR-PERF-01 |

### 6.2 Availability and release
| ID | Requirement |
|---|---|
| NFR-REL-01 | Releases cause no downtime: users keep working while a new version rolls out |
| NFR-REL-02 | A new version takes a small share of traffic first, grows in steps (20%, 50%, 100%), and rolls itself back when its error rate passes 5% or its 95th-percentile latency passes 500 ms (Decided) |
| NFR-REL-03 | A deliberately broken release that passes every test is rolled back automatically in production, and the rollback is recorded (Decided: this is the proof of NFR-REL-02) |
| NFR-REL-04 | Health checks tell the platform when a part is alive and when it is ready for traffic; they are never rate limited |

### 6.3 Security
| ID | Requirement |
|---|---|
| NFR-SEC-01 | Access tokens are never readable by browser scripts; the browser holds only an httpOnly, Secure session cookie |
| NFR-SEC-02 | Every input is validated on the server; a refusal names the field and never reveals internals (no stack traces, no SQL, no file paths) |
| NFR-SEC-03 | Errors use one machine-readable format (RFC 9457 problem details) for every client, whatever it asks to accept |
| NFR-SEC-04 | Sign-in and the public customer pages are rate limited per client; clients behind the platform's gateway are told apart by their real address, and a client cannot choose its own address |
| NFR-SEC-05 | Every response carries security headers: no MIME sniffing, no framing, a referrer policy, a content security policy |
| NFR-SEC-06 | Secrets are never stored in a repository in readable form |
| NFR-SEC-07 | Dependencies and images are scanned in CI; a fixable high or critical finding fails the build; every image has a bill of materials |
| NFR-SEC-08 | Containers run as non-root users, with read-only file systems where the product allows |

### 6.4 Accessibility and devices
| ID | Requirement |
|---|---|
| NFR-ACC-01 | WCAG 2.2 level AA on every page, light and dark, checked automatically on every page in CI |
| NFR-ACC-02 | Status is always text with colour, never colour alone |
| NFR-ACC-03 | Touch targets in the technician app are at least 44 by 44 px |
| NFR-ACC-04 | Every control is usable by keyboard with a visible focus, and a skip link leads past the navigation |
| NFR-ACC-05 | Supported: the last two versions of Chrome, Edge, Firefox and Safari; Safari and Chrome on phones; widths from 360 px to 2560 px |

### 6.5 Operability
| ID | Requirement |
|---|---|
| NFR-OPS-01 | Traces, metrics and logs from every part go to the owner's lab observability stack through OpenTelemetry |
| NFR-OPS-02 | Dashboards show requests, errors and latency per part, SLA breaches per tenant, and notification failures |
| NFR-OPS-03 | Alerts fire on a release rollback, an error rate above 2% for 5 minutes, and notification failures; each alert links a runbook |
| NFR-OPS-04 | Every running part reports its version (the commit it was built from) |

### 6.6 Data
| ID | Requirement |
|---|---|
| NFR-DATA-01 | Schema changes never break the running version: a change is added first, used next, and old parts are removed in a later release |
| NFR-DATA-02 | Times are stored in UTC and shown in the tenant's time zone |
| NFR-DATA-03 | Photos and signatures are kept for 2 years after job completion, then deleted (Proposed) |

---

## 7. Held back: the live dispatch board (FR-BOARD)

**Do not build this before the event.** It is the one story built live through aidlc, from intent to canary release,
in a 2-minute video. Version 1 contains none of it. The owner starts it.

**FR-BOARD-01 The board.** A timeline for today: one row per technician on shift, their assignments as blocks,
unassigned jobs in a side list ordered by SLA urgency.
**FR-BOARD-02 Drag to assign.** Dragging an unassigned job onto a technician's row at a time assigns it with a window
of the job type's usual length.
**FR-BOARD-03 What it costs.** While dragging, the board shows the estimated travel time from the technician's
previous job (from distance; no live traffic), and marks a clash with another assignment or with the shift's end.
**FR-BOARD-04 SLA countdowns.** Every block and unassigned job shows its SLA countdown, ticking.
**FR-BOARD-05 Live.** Every dispatcher's board and the technician's phone show a change within 2 seconds.
**FR-BOARD-06 Never double-booked.** A technician is never booked into two overlapping windows, even when several
dispatchers drag at the same moment.
```
AC1  Given two dispatchers dragging different jobs onto the same technician's 10:00 to 12:00 slot at the same moment
     When both drop
     Then exactly one assignment is saved; the other dispatcher sees "Already booked: JB-…" and the job returns to the list
AC2  Given 50 attempts racing for the same slot
     Then exactly one succeeds, every time
```
**FR-BOARD-07 Released safely.** The board is behind a feature switch, released by canary (NFR-REL-02), and
switched on for the demo tenant only.

---

## 8. Quality gates (must pass in CI on every change)

| Gate | Proves |
|---|---|
| Lint, formatting and type checks | Code meets the standards in each repository's rules file |
| Unit tests | Rules: the job status paths, SLA clocks and pauses, validation, permissions |
| Integration tests against a real database | Endpoints, tenant isolation below the application (FR-IAM-02 AC2), migrations, reliable notifications |
| Contract tests | The web app and the API cannot drift: the API's published contract is the source, and the web client is generated from it |
| Browser tests | The dispatcher flow; the technician flow at phone size, including offline and back; the customer page |
| Accessibility scan | NFR-ACC-01 on every page, light and dark |
| Performance budgets | NFR-PERF-02 and NFR-PERF-03 |
| Load smoke test | NFR-PERF-01 |
| Image scan and bill of materials | NFR-SEC-07 |
| Canary drill | NFR-REL-03, before the event |

---

## 9. Constraints set by the owner

These are decisions, not open choices. `aidlc-intent` records them as constraints; `aidlc-design` builds within them.

### 9.1 Technology (Decided)
- Web: Next.js 16 (React 19, TypeScript strict), acting as the back end for the front end: it signs users in and calls the API on their behalf.
- UI: Tailwind CSS v4 and shadcn/ui, themed by the tokens in 9.3.
- API: .NET 10, one service with clear internal modules (jobs, scheduling, technicians, customers and sites, SLA, notifications, tenancy and identity), plus a worker for background work.
- API contract: OpenAPI 3.1, published by the API.
- Data: PostgreSQL 17, one database, a tenant on every row, isolation enforced by the database.
- Identity: Keycloak 26, one organisation per tenant; OpenID Connect with PKCE.
- Live updates: SignalR, with a shared backplane so every server instance reaches every client.
- Files (photos, signatures, logos): S3-compatible object storage (MinIO locally).
- Email and text messages: local stand-ins (Mailpit, and a stub that records texts) behind one notifications interface.
- Feature switches: OpenFeature with flagd.
- Telemetry: OpenTelemetry to the owner's lab collector.

### 9.2 Repositories and identity (Decided)
- An aidlc workspace holding two repositories: `jobwright-app` (web, API, worker, Helm charts, decisions, rules) and `jobwright-gitops` (what runs where).
- Commits use the identity `prashantsharma18 <6386481+prashantsharma18@users.noreply.github.com>`. Never the work email.
- Nothing is pushed to GitHub or any remote until the owner asks. The repositories become private GitHub repositories later; images then go to GitHub Container Registry.

### 9.3 Look: Clean Enterprise (Decided)
Light neutrals, slate text, one confident blue. Inter, with tabular numerals for counts and times. Radius 8 px for
controls, 12 px for cards, full for pills. A sidebar on desktop; a bottom tab bar under 768 px.

| Token | Light | Dark |
|---|---|---|
| Page background | `#FFFFFF` | `#020617` |
| Subtle background | `#F8FAFC` | `#0F172A` |
| Primary text | `#0F172A` | `#F8FAFC` |
| Secondary text | `#64748B` | `#94A3B8` |
| Border | `#E2E8F0` | `#1E293B` |
| Brand | `#2563EB` | `#3B82F6` |
| Brand hover | `#1D4ED8` | `#60A5FA` |
| Brand subtle | `#E8EFFE` | `#172554` |
| Text on brand | `#FFFFFF` | `#020617` |

Status colours (text on background, light): urgent `#B91C1C` on `#FEE2E2`, high `#B45309` on `#FEF3C7`, success
`#15803D` on `#DCFCE7`, info `#1D4ED8` on `#DBEAFE`, scheduled `#6D28D9` on `#EDE9FE`; dark mode uses the same hues
at AA contrast.

Known contrast trap: brand blue text on brand-subtle background fails AA (4.48:1 light, 3.99:1 dark). Use brand
hover for text on brand-subtle.

### 9.4 Platform and delivery (Decided)
- Runs on the owner's machine: k3d (Kubernetes in Docker) on Docker Desktop under WSL2, with a local registry and a local Git server that the deployment tool reads.
- Traffic enters through the Gateway API, implemented by Traefik.
- Argo CD deploys only from Git. Staging updates automatically when a change merges; production changes only through a reviewed pull request in `jobwright-gitops`, and only to a version staging runs (or a rollback to a version production ran before).
- Argo Rollouts releases by canary (NFR-REL-02), judging error rate and latency from an in-cluster Prometheus.
- Secrets for the cluster are Sealed Secrets.
- CI: lint and format, type check, unit, integration, contract, build images, scan and bill of materials, browser tests; GitHub Actions pinned to commit SHAs, read-only tokens by default, Renovate for updates.

### 9.5 Machine and safety rules (Decided)
- **Never touch the owner's real clusters.** The machine's default kubeconfig holds live work clusters. Jobwright uses its own kubeconfig, and every script refuses to act on anything but the local k3d cluster.
- **Agents never change the cluster.** No `kubectl apply`, Helm install or rollout commands by an agent. A change reaches the cluster only through a merged Git change; the owner runs one-time setup and merges release pull requests.
- **Ports.** The owner's AI lab holds host ports 3000, 3001, 4318, 4444, 9090 and 9091. Jobwright uses others, all bound to 127.0.0.1.
- **Memory.** The cluster and its services fit in about 6 GB beside the lab (a 16 GB machine).
- **No external MCP servers** are added for this work.
- **New dependencies** (packages, tools, images) need the owner's agreement first.

---

## 10. Success criteria for the whole product

1. A dispatcher takes a call, creates a job, assigns a technician and sees the SLA; the technician completes it on a phone, going offline and back; the customer follows it from the link. In light and dark, on desktop and phone.
2. Every gate in section 8 is green in CI.
3. A deliberately bad release rolls itself back in production (NFR-REL-03).
4. The demo tenant looks like a real business on a normal working day.
5. The live dispatch board (section 7) can be built through aidlc from intent to canary release, and the story fits a 2-minute video.

---

## 11. Assumptions
- The owner's lab observability stack (collector, Grafana, Loki, Mimir) is running when telemetry is tested.
- Map tiles come from OpenStreetMap and need internet access; travel times are estimates from distance.
- One person (the owner) acts as product owner, tech lead and approver for this demo.

## 12. Open questions
| ID | Question | Owner | Blocks |
|---|---|---|---|
| Q1 | Which tracker holds the stories: the lab Jira (its MCP server was not connected on 2026-11-06) or files in the workspace? | Prashant Sharma | Story keys |
| Q2 | Confirm every **Proposed** value in this document (session times, SLA defaults and thresholds, link expiry, retention, offline limits) | Prashant Sharma | `aidlc-specify` for each capability |
| Q3 | Does the greeting follow the tenant's time zone (proposed) or the viewer's? | Prashant Sharma | FR-OVR-03 |
| Q4 | Is web code formatting enforced (it needs a formatter such as Prettier, a new dependency)? | Prashant Sharma | Quality gate "formatting" |
