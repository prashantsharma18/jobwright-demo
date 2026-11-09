<!-- aidlc template: spec -->
# NEW-jobwright · Jobwright: the new system and its foundations · specification

Spec owner: Prashant Sharma · Tier: 3 · Version: v2

## For the reviewer
- **Asked of:** The spec owner, to approve the criteria and decide every edge case; a specialist reviews at Tier 3 (here the same person: known limit, intent answer I3)
- **Record it:** The tracker is files in this workspace, so on this file's pull request: "Spec v2 approved by Prashant Sharma"
- **Tier and main risk:** 3: one company seeing or changing another's data, or a weakness in signing people in or in releasing
- **Changed since the last version:** v2, amendment 1 (2026-11-09, [amendment-1.md](amendment-1.md)): requirements 8, 9.2 and 9.4 move the repositories, images and CI to GitHub. New rules and known limits; AC23, AC27, E17 and E23 changed; E28 to E31 added, E32 dropped; the conflicts of E31 with AC23 and E30 resolved by the spec owner

Sources: [intent v2](intent.md) and the
[requirements v1.0](../../docs/requirements/2026-11-08-jobwright-requirements.md). Requirement IDs (FR-, NFR-) are
the requirements'; AC and E numbers are this spec's. Route G: this spec states the system and the first criteria it
must meet. Slice 0 builds only the skeleton; these criteria are built after `aidlc-onboard` confirms each repository.

## Outcome
People of several service companies sign in to one Jobwright, each sees only their own company, is offered only what
their role allows, and works in an application shell that suits desktop and phone, light and dark. Every change is
checked by the section 8 gates that apply to this increment, and a new version reaches users in steps and reverses
itself when it misbehaves. Every later capability (increments 2 to 4) builds on this.

## Scope and non-goals
In (increment 1, Foundations, requirements section 4):
- Signing in and out, sessions, tenants, roles: FR-IAM-01 to 03.
- The application shell: FR-SHELL-01 to 03, with the look of section 9.3.
- Creating a tenant and its first admin, by a command the owner runs on the local cluster (S4).
- The platform and delivery: the qualities of section 6 in AC12 to AC28, with the release drill in staging (S11), and
  the constraints of section 9.
- The CI gates of section 8 in AC27 (S11).

Non-goals (from intent "Not now"):
- Increment 2, Office: customers, sites, assets, jobs, technicians, SLAs, Overview figures, users and roles
  administration, activity and audit. A later story.
- Increment 3, Field and customer: the technician app, the customer page, notifications, live updates, personal data
  export and erasure. A later story; it carries intent answer I4.
- Increment 4, Hardening (S11): realistic demo data (FR-DEMO), browser tests of the later flows, the production
  canary drill (NFR-REL-03), the load smoke test (NFR-PERF-01, 04), page speed with demo data (NFR-PERF-02),
  technician touch targets (NFR-ACC-03), dashboards, alerts and their runbooks (NFR-OPS-02, 03), and retention
  (NFR-DATA-03). A later story.
- A platform role for creating tenants: none in version 1 (S4).
- The live dispatch board (section 7): nothing of it, not even switched off.
- Everything section 4 lists as not in scope.

## Rules and constraints
- A user has exactly one role in a tenant: dispatcher, technician or tenant admin (set by Prashant Sharma, section 2).
- Accounts live in Jobwright's own identity provider, one organisation per tenant; people are invited by email; no
  company brings its own identity provider in version 1 (set by Prashant Sharma, section 9.1, S4).
- Only the owner creates a tenant and its first admin, with one command run on the local cluster; the demo seed uses
  the same command; no product role can create a tenant (set by Prashant Sharma, S4).
- No email or text message leaves the machine; every message goes to a local stand-in that captures it (set by
  Prashant Sharma, FR-NTF-03).
- Formatting is enforced in CI for the web code (with Prettier, approved as a new dependency; design pins its
  version) and for the .NET code (set by Prashant Sharma, S5, requirements Q4).
- Both repositories are private GitHub repositories; there is no local Git server (set by Prashant Sharma,
  amendment 1).
- Argo CD in the local cluster reads `jobwright-gitops` from GitHub with a read-only deploy key; images live in GitHub
  Container Registry and the cluster pulls them with a pull secret; both are sealed secrets. The cluster holds no
  credential that can write to GitHub (set by Prashant Sharma, amendment 1, A4).
- The section 8 gates run in GitHub Actions on every pull request and report pass or fail on it. Nothing blocks a
  merge on the current plan: known limit KL1 (set by Prashant Sharma, amendment 1, A1).
- Prashant Sharma pushes branches and opens every pull request a person or an agent writes; named automation opens
  Renovate's updates and the production promotion pull requests. Author and approver are the same person: known limit
  KL2 (set by Prashant Sharma, amendment 1, A2).
- CI stores no deployment secret. It uses only GitHub's built-in token: read-only on pull requests; on `main` of
  `jobwright-app`, only the write needed to push images to GitHub's registry, and the delete of images older than 30
  days (E30); on `main` of `jobwright-gitops`, a scheduled workflow's write to its own repository, to commit staging's
  new image versions read from the registry. A stored secret is a new decision (set by Prashant Sharma, amendment 1,
  E31, E30).
- What each role may do is the permissions table of section 2; in this increment that means only Settings is
  restricted (to tenant admins) (set by Prashant Sharma, FR-IAM-03, FR-SHELL-01 AC3).
- A user never sees, changes or learns about another tenant's data; asking for another tenant's record by its
  identifier gets the same answer as a record that does not exist (set by Prashant Sharma, FR-IAM-02).
- Isolation is enforced by the database below the application, so a query that omits the tenant still returns only
  the current tenant's rows (set by Prashant Sharma, FR-IAM-02 AC2, section 9.1).
- The browser never holds an access token readable by scripts; it holds only an httpOnly, Secure session cookie (set
  by Prashant Sharma, NFR-SEC-01).
- Every input is validated on the server; a refusal names the field and never shows internals (stack traces, SQL,
  file paths) (set by Prashant Sharma, NFR-SEC-02).
- Every error, for every client and whatever it asks to accept, is one machine-readable problem format, RFC 9457
  (set by Prashant Sharma, NFR-SEC-03).
- Sign-in is rate limited per client; behind the gateway, clients are told apart by their real address, and a client
  cannot choose its own address (set by Prashant Sharma, NFR-SEC-04). Health checks are never rate limited (NFR-REL-04).
- Every response carries: no MIME sniffing, no framing, a referrer policy, a content security policy (set by Prashant
  Sharma, NFR-SEC-05).
- No secret is stored in either repository in readable form (set by Prashant Sharma, NFR-SEC-06).
- Times are stored in UTC and shown in the tenant's time zone (set by Prashant Sharma, NFR-DATA-02).
- A schema change never breaks the running version: added first, used next, old parts removed in a later release
  (set by Prashant Sharma, NFR-DATA-01).
- A new version takes traffic in steps of 20%, 50% and 100%, and rolls back by itself when its error rate passes 5%
  or its 95th-percentile latency passes 500 ms (set by Prashant Sharma, NFR-REL-02).
- Staging updates by itself when a change merges; production changes only through a reviewed pull request in
  `jobwright-gitops`, and only to a version staging runs, or back to one production ran before (set by Prashant
  Sharma, section 9.4).
- Every script that acts on a cluster refuses any cluster but the local one, and agents never change the cluster
  (set by Prashant Sharma, section 9.5).
- Jobwright never uses host ports 3000, 3001, 4318, 4444, 9090 or 9091, and binds every port it opens to 127.0.0.1
  (set by Prashant Sharma, section 9.5).
- The cluster and its services fit in about 6 GB of memory (set by Prashant Sharma, section 9.5; "about" needs a
  number: E21).
- New dependencies (packages, tools, images) need the owner's agreement first (set by Prashant Sharma, section 9.5);
  approving the design's `new-system.md` is that agreement for the ones it lists.
- The technology of section 9.1 and the platform of section 9.4 are Decided (set by Prashant Sharma); `aidlc-design`
  builds within them.

**Conflicts flagged, not resolved:** none. The conflicts of amendment 1 (E28, E31, E32 against A1 and A2; then E31
against AC23 and E30) were resolved by the spec owner on 2026-11-09 (Decisions recorded).
- Related fact, not a conflict: "Pull request authors cannot approve their own pull requests" (docs.github.com). Pull
  requests Prashant Sharma opens therefore merge unapproved (KL2); the promotion pull requests, opened by automation,
  can be approved by him, so "production only through a reviewed pull request" can still hold.

## Context and contracts
Nothing exists yet; every contract here is new, and both of its ends are owned by Prashant Sharma.

| Contract | Provider -> consumers | Note |
|---|---|---|
| The API's published contract (OpenAPI 3.1, section 9.1) | API -> web app (its generated client) | Both in `jobwright-app`; contract tests in section 8 |
| Error format (RFC 9457) | API and web app -> every client | NFR-SEC-03 |
| Sign-in (OpenID Connect with PKCE, section 9.1) | identity provider -> web app | Jobwright's own identity provider, one organisation per tenant (S4) |
| Tenant command | the owner -> the identity provider and the database | Run on the local cluster only (AC22, AC29) |
| Health: alive and ready (NFR-REL-04) | every running part -> the platform and the canary analysis | |
| Version: the commit each part was built from (NFR-OPS-04) | every running part -> people and the release record | |
| Released versions: images and charts by version | `jobwright-app` -> `jobwright-gitops` | A cross-repository contract; `aidlc-design` defines it |
| Telemetry (OpenTelemetry, NFR-OPS-01) | every running part -> the owner's lab collector | The lab is outside both repositories, owned by Prashant Sharma |

Domain of this increment:

| Entity | Holds | States and events |
|---|---|---|
| Tenant | name, time zone, the phone shown to customers (FR-ADM-03; logo later) | created by the owner's tenant command (AC29) |
| Person | name, email, sign-in identity | signs in, signs out |
| Membership | person, tenant, one role | added, role changed, removed (E2) |
| Session | person, current tenant, sign-in time, last activity | started, used, ended by sign-out, ended by idle or absolute expiry (AC3), ended by removal (E2) |
| Theme choice | light, dark or follow the device | kept per device or per person: E10 |

Inputs:

| Input | Type | Source | Who controls its value |
|---|---|---|---|
| The page asked for before sign-in (return address) | address | the browser | the user, or anyone who sends them a link: E4 |
| Identity claims: who, which tenants, which role | signed claims | the identity provider | the owner's tenant command for the first admin; later, the tenant admin's invitations (FR-ADM-01, increment 2) |
| The tenant command's inputs: tenant name, time zone, first admin's name and email | text | the owner's terminal | the owner only |
| The chosen tenant, for a person in several (E3) | identifier | the browser | the user: E5 |
| A record identifier in a request | identifier | the browser | the user: AC4 |
| The client's address, for rate limiting | network address | the gateway | the gateway only, never the client (rule above) |
| The theme choice | one of three values | the browser | the user |
| The cluster a script acts on | kubeconfig context | the machine | the person running the script: AC22, E24 |

## Acceptance criteria
Each names the repository whose tests prove it.

AC1 Sign-in returns to the page asked for (FR-IAM-01 AC1) · `jobwright-app`
    Given a dispatcher of Apex without a session
    When  they open the Settings address, or any other page
    Then  they are sent to sign in, and after signing in they land on the page they asked for; a technician and a
          tenant admin behave the same, each landing where their role allows (AC6)

AC2 Sign-out ends the session on the server (FR-IAM-01 AC2) · `jobwright-app`
    Given a signed-in user
    When  they choose "Sign out", and then replay the old session cookie
    Then  the session no longer exists on the server, the replayed request is sent to sign in, and nothing of
          the tenant is returned

AC3 Sessions expire (FR-IAM-01 AC3; values decided in E1) · `jobwright-app`
    Given a session idle for 60 minutes, and another started 12 hours ago and active throughout
    When  each makes a request
    Then  each is sent to sign in again; at 59 minutes idle and at 11 hours 59 minutes the request succeeds

AC4 Another tenant's record is "not found" (FR-IAM-02 AC1; records: memberships, tenant settings and one tenant-owned table, S12) · `jobwright-app`
    Given tenant-owned records in Apex and Brightline
    When  a user of Apex lists or searches them, or asks for a Brightline record by its identifier
    Then  only Apex records are returned, and the Brightline identifier gets exactly the answer an identifier that
          does not exist gets: same status, same body shape, no hint that it exists

AC5 Isolation holds below the application (FR-IAM-02 AC2; the same records as AC4, S12) · `jobwright-app`
    Given tenant-owned records in Apex and Brightline, and the current tenant is Apex
    When  a read that does not filter by tenant runs against the real database
    Then  only Apex rows are returned; with no current tenant set, no rows are returned

AC6 Each role is offered only what it may do (FR-IAM-03 AC2, FR-SHELL-01 AC3) · `jobwright-app`
    Given a dispatcher and a technician of Apex
    When  the navigation renders, and when each opens the Settings address directly
    Then  Settings is not offered, and opening it is refused as forbidden with a problem response; a tenant admin
          is offered Settings and opens it

AC7 Desktop navigation (FR-SHELL-01 AC1) · `jobwright-app`
    Given a screen 768 px wide or wider
    When  any page loads
    Then  a left sidebar lists every section the role may use (Overview, Jobs, Technicians, Customers, Assets,
          Reports, and Settings for a tenant admin) and marks the current one; at 767 px, AC8 applies instead
          (what an unbuilt section shows: E11)

AC8 Phone navigation (FR-SHELL-01 AC2) · `jobwright-app`
    Given a screen narrower than 768 px, down to 360 px
    When  any page loads
    Then  a bottom tab bar shows Overview, Jobs, Technicians and Customers, and every other section the role may use
          is reachable from an "All sections" control in the top bar

AC9 Light and dark (FR-SHELL-02) · `jobwright-app`
    Given a device set to dark, and a user who has made no choice
    When  any page loads
    Then  it is dark; after the user chooses light and reloads, or comes back later, it is light; after they
          choose "follow the device", it follows the device again (where the choice is kept: E10)

AC10 The top bar names the tenant and the user (FR-SHELL-03) · `jobwright-app`
    Given a tenant admin of Apex
    When  any page loads
    Then  the top bar shows "Apex Facility Services", the user's name and "Tenant admin" (long names: E19)

AC11 The look is section 9.3 · `jobwright-app`
    Given the shell in light and in dark
    When  its colours, type and radii are compared with section 9.3
    Then  they use exactly the tokens of the table, Inter with tabular numerals for counts and times, radius 8 px
          for controls, 12 px for cards and full for pills; no text uses brand blue on brand-subtle

AC12 Every page is accessible (NFR-ACC-01, 02, 04, 05) · `jobwright-app`
    Given every page of this increment, in light and in dark, at 360 px and at 1440 px
    When  the automated scan runs in CI
    Then  it finds no WCAG 2.2 AA violation; every control is reachable by keyboard with a visible focus, a skip
          link leads past the navigation, no status is shown by colour alone, and no width from 360 px to 2560 px
          scrolls sideways

AC13 Errors are one format and reveal nothing (NFR-SEC-02, 03) · `jobwright-app`
    Given a request with an invalid field, a request that fails inside the server, and a request asking to
          accept HTML or XML
    When  each is answered
    Then  each answer is an RFC 9457 problem response; the first names the field; none contains a stack trace,
          SQL, a file path or a class name

AC14 No readable token in the browser (NFR-SEC-01) · `jobwright-app`
    Given a signed-in user
    When  any page's scripts read cookies and storage
    Then  they find no access, refresh or identity token; the only session cookie is httpOnly and Secure

AC15 Security headers on every response (NFR-SEC-05) · `jobwright-app`
    Given any response of the web app or the API, including errors, redirects and health
    When  its headers are read
    Then  it forbids MIME sniffing and framing and carries a referrer policy and a content security policy

AC16 Sign-in is rate limited per real client (NFR-SEC-04, NFR-REL-04; the limit: E9) · `jobwright-app`, `jobwright-gitops`
    Given one client address that exceeds the sign-in limit
    When  it tries again, also with a forged forwarded-address header
    Then  it is refused with a problem response saying when to retry, while another address is not refused, and
          health checks are never refused

AC17 Alive and ready (NFR-REL-04) · `jobwright-app`
    Given a running part whose database is unreachable
    When  the platform asks whether it is alive and whether it is ready
    Then  it answers alive and not ready, and takes no traffic until it is ready again

AC18 Every part reports its version (NFR-OPS-04) · `jobwright-app`
    Given a running web app, API and worker built from commit C
    When  each is asked its version
    Then  each answers C, and its telemetry carries C

AC19 Telemetry reaches the lab and holds no personal data (NFR-OPS-01; personal data in this increment: E26) · `jobwright-app`
    Given a sign-in, a page view and an error
    When  their traces, metrics and logs are read in the lab
    Then  each part's telemetry is there, and none holds a person's name, email or session cookie

AC20 Releases cause no downtime (NFR-REL-01) · `jobwright-app`, `jobwright-gitops`
    Given a signed-in user working while a new version rolls out, and while it rolls back
    When  they keep using the shell
    Then  no request fails because of the release, and they are not asked to sign in again

AC21 A new version takes traffic in steps and rolls itself back (NFR-REL-02; proved here by a drill in staging, the production drill is increment 4: S11) · `jobwright-gitops`, `jobwright-app`
    Given a release of a version whose error rate passes 5%, or whose 95th-percentile latency passes 500 ms, at any step
    When  the release reaches that step
    Then  it rolls back to the previous version by itself, no further step is taken, and the rollback is recorded;
          a healthy version goes 20%, 50%, 100%

AC22 Scripts refuse any cluster but the local one (section 9.5) · `jobwright-gitops`, `jobwright-app`
    Given the machine's default kubeconfig, or any context that is not the local Jobwright cluster
    When  any script in either repository that acts on a cluster is run with it
    Then  it stops before acting, says which cluster it refused, and changes nothing

AC23 Production changes only by review, to a known version (section 9.4; A4) · `jobwright-gitops`
    Given a production promotion pull request, and the rollout record Prashant Sharma keeps in it: each completed or
          aborted rollout of staging and production since the last promotion, with its digest
    When  its checks run
    Then  it fails, naming the version, when a digest is neither recorded as a completed staging rollout nor recorded
          as a completed production rollout in an earlier promotion; it passes otherwise; a change merged to
          `jobwright-app` updates staging with no person acting, after a delay of a few minutes set by the design's
          schedule. (The records are the person's statement, not facts
          from the cluster: known limit KL3. The check reports; on the current plan it does not block: KL1.)

AC24 Nothing secret, nothing unscanned, nothing as root (NFR-SEC-06, 07, 08) · `jobwright-app`, `jobwright-gitops`
    Given a change that adds a readable secret, a dependency or image with a fixable high or critical finding, or a
          container set to run as root
    When  CI runs
    Then  it fails, naming the finding; every image built has a bill of materials; every container runs as a
          non-root user, with a read-only file system unless the design names why not

AC25 Schema changes never break the running version (NFR-DATA-01) · `jobwright-app`
    Given the previous released version and a change that alters the schema
    When  the previous version's integration tests run against the new schema
    Then  they pass

AC26 The first load stays within budget (NFR-PERF-03) · `jobwright-app`
    Given the budget for the JavaScript of the Overview's first load, set by the plan
    When  CI builds the web app
    Then  the build fails if the Overview's first load sends more than the budget

AC27 Every applicable gate runs on every change (section 8; these gates: S11; A1) · `jobwright-app`, `jobwright-gitops`
    Given any pull request in either repository
    When  GitHub Actions runs
    Then  lint, formatting (web and .NET, S5) and type checks, unit, integration against a real database, contract,
          browser, accessibility, budget, image scan and bill of materials each report pass or fail on the pull
          request; a red or missing gate is visible on it; merging is not blocked on the current plan (KL1); the load
          smoke test and the canary drill are increment 4's

AC28 It fits the machine (section 9.5) · `jobwright-gitops`
    Given the local cluster with every service of this increment running and idle, beside the owner's lab
    When  memory is measured
    Then  the total is at most the number set in E21 (about 6 GB), and no Jobwright port is one of the reserved
          ports or bound to an address other than 127.0.0.1

AC29 The owner creates a tenant and its first admin (S4) · `jobwright-app`
    Given the local cluster, and a tenant name, a time zone, and the first admin's name and email
    When  the owner runs the tenant command
    Then  the tenant exists with its own organisation in the identity provider, the first admin is a tenant admin of
          it, their invitation is captured by the local mail stand-in, and after accepting it they sign in and see
          only that tenant; a missing or invalid input is refused, naming the field, and nothing is created; run
          against any other cluster, it refuses as AC22 (a duplicate name: E27)

Assumptions made (A1 settled by S4 and S9; A2 to A4 confirmed by Prashant Sharma, 2026-11-08):
- A1 Two tenants, Apex Facility Services and Brightline Services, with one user per role, are seeded with the tenant
  command (AC29) so this increment can be signed in to and isolation shown. The full demo data is increment 4
  (FR-DEMO). Settled by S4 and S9; the owner checks the names before the demo.
- A2 "Every page of this increment" (AC12) means the sign-in return, every section's page as E11 decides, and
  Settings.
- A3 The worker exists from this increment, with no work yet, so that health, version and telemetry are proved for
  it now.
- A4 Production exists from this increment, so that the release in AC20, AC21 and AC23 is a real one.

## Edge cases (each with a decided behaviour and its decider)
E1 to E27 were decided by Prashant Sharma on 2026-11-08, accepted as proposed with no exceptions, each in the role it
names. E17 v2, E23 v2, E29 and E30 were decided by Prashant Sharma on 2026-11-09, accepted as proposed (amendment 1);
E28 and E31 as he reworded them, and E32 dropped, the same day; E30's fallback and E31's two writes on `main` the same
day.

E1 Session limits -> 60 minutes idle, 12 hours absolute, as the requirements proposed (decided by: Prashant
   Sharma, as risk owner)
E2 A person removed from a tenant, or whose role changes, while signed in -> their next request is refused, the
   session ends, and they sign in again (FR-IAM-03 E1; decided by: Prashant Sharma, as risk owner)
E3 A person in two tenants -> after sign-in they choose the tenant and can switch from the top bar; a switch starts a
   fresh view and nothing of the first tenant stays on screen (FR-IAM-03 E2; decided by: Prashant Sharma,
   as spec owner and risk owner)
E4 The page asked for before sign-in is on another site, or is not a page of Jobwright -> it is ignored and the user
   lands on Overview (decided by: Prashant Sharma, as risk owner)
E5 A request names a tenant the session's person does not belong to, or not the session's current tenant -> "not
   found", as AC4 (decided by: Prashant Sharma, as risk owner)
E6 A person disabled in the identity provider, or with no membership in any tenant, signs in -> no tenant data is
   shown, and a page says "You don't have access to Jobwright. Ask your administrator." (decided by:
   Prashant Sharma, as spec owner)
E7 The identity provider is unavailable -> people without a session see "Sign-in is unavailable. Try again in a few
   minutes."; sessions already started keep working until they expire (decided by: Prashant Sharma, as risk
   owner)
E8 The session store is unavailable -> requests are sent to sign in rather than served without a session (decided
   by: Prashant Sharma, as risk owner)
E9 The sign-in rate limit -> 10 attempts a minute per client address, then refused for the rest of the minute (the
   requirements give no number; decided by: Prashant Sharma, as risk owner)
E10 Where the theme choice is kept -> on the device, so a phone and a desktop can differ (the alternative, per person
   on every device, was declined; decided by: Prashant Sharma, as spec owner)
E11 A section with nothing in it yet in this increment (Jobs, Technicians, Customers, Assets, Reports; Overview's
   figures) -> the section is offered and shows an empty state that names what it will hold, so the shell can be
   judged whole (the alternative, offering only Overview and Settings until each section is built, was
   declined; decided by: Prashant Sharma, as spec owner)
E12 Two sign-ins of one person in two browsers -> both sessions are valid and independent; signing out of one leaves
   the other (decided by: Prashant Sharma, as risk owner)
E13 A new version that never becomes ready -> it takes no traffic, the release stops and rolls back by itself, and it
   is recorded (decided by: Prashant Sharma, as release owner)
E14 A rollback, or two versions side by side -> signed-in sessions survive both, as AC20 (decided by:
   Prashant Sharma, as release owner)
E15 A release with too little traffic at a step to judge its error rate or latency -> the step waits, up to a limit
   the design sets, then the release rolls back rather than promoting a version it could not judge (decided by:
   Prashant Sharma, as release owner)
E16 The lab collector is unavailable -> the product keeps working; telemetry is dropped after a bounded buffer and no
   user sees a difference (decided by: Prashant Sharma, as spec owner)
E17 (v2) GitHub or its container registry is unavailable -> staging and production do not change; nothing already
   running stops; a pod restarted meanwhile starts from the image already on the node, or waits; nothing ever falls
   back to an unpinned image (decided by: Prashant Sharma, as release owner, 2026-11-09)
E18 A forwarded-address header from a client, rather than the gateway -> ignored for rate limiting and in logs
   (Decided in NFR-SEC-04; listed so the test names it; decided by: Prashant Sharma)
E19 A tenant or person name longer than the top bar holds -> it is shortened with an ellipsis and the full name is
   available to pointer, keyboard and screen reader (decided by: Prashant Sharma, as spec owner)
E20 A record identifier that is malformed, empty or very long -> refused as invalid, naming the field, before any
   lookup (decided by: Prashant Sharma, as risk owner)
E21 "About 6 GB" -> at most 6 GB for the cluster and everything in it, measured idle with the increment's services
   running (decided by: Prashant Sharma, as spec owner)
E22 A port Jobwright needs is already in use on the machine -> the setup stops and names the port, instead of
   choosing another (decided by: Prashant Sharma, as spec owner)
E23 (v2) A secret is found in a commit -> CI fails; the secret is treated as exposed and rotated, not only removed;
   and every commit is scanned on the machine before it is pushed, and a found secret stops the push (whether GitHub's
   own push protection is also used depends on the plan) (decided by: Prashant Sharma, as risk owner, 2026-11-09)
E24 A kubeconfig context whose name looks like the local cluster's but whose server is elsewhere -> refused, as AC22:
   the check is on the server the context points to, not its name (decided by: Prashant Sharma, as risk
   owner)
E25 A browser with scripts turned off -> the sign-in still works, and pages say scripts are needed (decided by:
   Prashant Sharma, as spec owner)
E26 Personal data in this increment -> people's names and emails, and client addresses used for rate limiting, are
   personal data and stay out of logs, traces and metrics now, ahead of FR-PRV in increment 3 (decided by:
   Prashant Sharma, as privacy reviewer)
E27 The tenant command is run with a tenant name that already exists -> it is refused, naming the existing tenant,
   and nothing changes (added with AC29 after decision S4; decided by: Prashant Sharma, as spec owner)
E28 A pull request whose checks have not all run (GitHub Actions down, or the month's included compute used up), or
   have failed -> it is not merged until they pass; Prashant Sharma keeps this rule himself under KL1 (decided by:
   Prashant Sharma, as spec owner and risk owner, 2026-11-09)
E29 The deploy key or the pull secret is revoked, expired or wrong -> Argo CD reports the application as unable to sync
   and changes nothing; running pods keep running; new pods wait; a release stops and rolls back by its analysis
   (E13); the key is replaced with a new sealed secret, never typed into the cluster (decided by: Prashant Sharma, as
   risk owner, 2026-11-09)
E30 The registry's storage limit is reached -> the image push fails and staging does not change; a retention job
   deletes images older than 30 days that no environment and no rollout record names, and never one they name
   (decided by: Prashant Sharma, as release owner, 2026-11-09). If the design finds that GitHub's built-in token
   cannot delete them, the fallback applies: Prashant Sharma deletes old images by hand when storage runs low, and
   this edge case is reworded
E31 A workflow asks for a deployment secret -> there is none: CI stores no deployment secret, and uses only GitHub's
   built-in token: read-only on pull requests; on `main`, only the writes the rule above lists (push images, delete
   images older than 30 days, and `jobwright-gitops`'s scheduled commit of staging's versions); needing a stored
   secret is a new decision (decided by: Prashant Sharma, as risk owner, 2026-11-09)
E32 Dropped (Prashant Sharma, 2026-11-09): under A2 there is no `aidlc-agent` account. The number is not reused.

Known limits (amendment 1, accepted by Prashant Sharma, 2026-11-09):
- KL1 (A1): on GitHub Free, private repositories have no protected branches, required reviews or code owners; the
  gates report but block nothing. Moving to GitHub Pro turns enforcement on.
- KL2 (A2): Prashant Sharma authors and merges his own pull requests; GitHub records no approval for them, because an
  author cannot approve his own. As intent I3.
- KL3 (A4): the rollout records AC23 checks are written by Prashant Sharma in each promotion pull request; the cluster
  writes nothing to GitHub.

## Verification
Test levels are proposed; `aidlc-plan` names each test.

| Criterion | How it will be proved |
|---|---|
| AC1, AC2, AC3 | Browser tests against the real identity provider; integration tests for expiry with a controlled clock |
| AC4, AC5 | Integration tests against a real database, with two seeded tenants |
| AC6 | Integration test of the refusal; browser test of the navigation |
| AC7, AC8, AC9, AC10 | Browser tests at 1440 px and 360 px, light and dark |
| AC11 | Unit test of the tokens against section 9.3; visual check in the browser test screenshots |
| AC12 | Automated accessibility scan of every page in CI, light and dark; keyboard browser test |
| AC13, AC15 | Integration tests on every route class (success, error, redirect, health) |
| AC14 | Browser test reading cookies and storage after sign-in |
| AC16 | Integration test through the gateway, with a forged header |
| AC17, AC18 | Integration tests; a check in the deployed staging |
| AC19 | A check in staging against the lab, with a search for seeded personal values |
| AC20, AC21 | A release drill in staging: a healthy version and a deliberately failing one (the production drill is increment 4, S11) |
| AC22, AC24 | Script tests with a fake non-local context; CI checks with planted findings |
| AC23 | A GitHub Actions check in `jobwright-gitops` that reads the rollout records, with fixture pull requests |
| AC25 | CI runs the previous release's integration tests against the new schema |
| AC26 | CI budget check |
| AC27 | Every gate reports on fixture pull requests in GitHub Actions; one of each made red on purpose; enforcement after GitHub Pro (KL1) |
| AC28 | A measurement script on the local cluster; a check of the port bindings |
| AC29 | Integration test of the tenant command against the local cluster's identity provider and database; the invitation read from the mail stand-in |

## Decisions recorded
All by Prashant Sharma, 2026-11-08. Each is written into the rules and criteria above; this table is the record.

| ID | Decision |
|---|---|
| S4 | (a): accounts live in Jobwright's own identity provider, one organisation per tenant, people invited by email (already Decided in requirements 9.1). Tenants and their first admin are created by a command the owner runs on the local cluster; the demo seed uses the same command. No new platform role in version 1. |
| S5 | Web formatting is enforced in CI. The formatter (Prettier) is approved as a new dependency; design pins its version. |
| S11 | This story proves AC12 to AC28, with the release drill (AC21) in staging. The production drill, the load smoke test, page speed with demo data, technician touch targets, dashboards and alerts, and retention move to increment 4. |
| S12 | Memberships, tenant settings and one tenant-owned table prove isolation in this increment. |
| S9 | The names "Apex Facility Services" and "Brightline Services" stay for now; the owner checks before the demo that they are not real businesses. |
| Q-date | No event date is fixed; the schedule is open. |
| E27 | Accepted as proposed. |
| A2 to A4 | Confirmed as written. |
| Amendment 1 | Requirements 8, 9.2 and 9.4 move to private GitHub repositories, GitHub Container Registry and GitHub Actions (Prashant Sharma, 2026-11-09) |
| A1 | GitHub Free for now; the gates report and block nothing (KL1); GitHub Pro later turns enforcement on |
| A2 | No separate account: Prashant Sharma pushes and opens every pull request; named automation opens Renovate's updates and the production promotion (KL2) |
| A4 | Prashant Sharma records each completed or aborted rollout in the production promotion pull request; the cluster gets no write credential (KL3) |
| E17 v2, E23 v2, E29, E30 | Accepted as proposed |
| E28 | Reworded: a pull request whose checks have not all run, or have failed, is not merged until they pass; kept by Prashant Sharma under KL1 |
| E31 | CI stores no deployment secret; only GitHub's built-in token, read-only on pull requests; a stored secret is a new decision |
| E31 against AC23 | (a): a scheduled workflow in `jobwright-gitops` reads the new image versions from GitHub's registry and commits them to its own `main` with its own built-in token; no stored secret; staging still updates by itself; a delay of a few minutes after a merge is fine |
| E31 against E30 | (a): the retention job may delete images older than 30 days, on `main` only; if the built-in token cannot, fallback (b): Prashant Sharma deletes old images by hand when storage runs low, and E30 is reworded |
| Tier 3 review of AC29 and E27 | Covered by Prashant Sharma's v2 review, 2026-11-09 |
| E32 | Dropped: no `aidlc-agent` under A2 |
| Tier 3 review of v2 | Security, privacy and release changes of amendment 1 reviewed by Prashant Sharma, 2026-11-09, under known limit I3 |

## Open questions
| ID | Question | Owner | Blocks |
|---|---|---|---|
| S9 | Check that "Apex Facility Services" and "Brightline Services" are not real businesses. | Prashant Sharma | the demo, not this spec |

## Definition of Ready (Tier 3)
- [x] Clear actor, observable behaviour, known constraints, examples, testable criteria.
- [x] Tier 2+: non-goals written (done); every edge case decided (E1 to E31, E32 dropped, 2026-11-09); contract changes agreed
      with consumers (no other team; the cross-repository contract is defined in design); questions owned (done:
      every open question has an owner).
- [x] Tier 3: specialist reviewed: security, privacy and release reviewed for this spec by Prashant Sharma on
      2026-11-08, under the known limit of intent answer I3 (the reviewer is also the product owner, tech lead and
      approver; no independent specialist reviewed it). Gap: that review was given before AC29 and E27 (the tenant
      command, which creates tenants and their first admins) were written; the reviewer confirms it covers them.
      Every risk decision names its decider (done); data in the agent's context classified (done: the requirements
      and this spec hold no personal data of real people; the owner's name and a no-reply address only; seeded
      people are invented, S9).
      v2: the security, privacy and release changes of amendment 1 (credentials between the cluster and GitHub,
      secrets, gates) reviewed by Prashant Sharma on 2026-11-09, under the known limit of intent answer I3; that review
      also covers AC29 and E27 (the reviewer, 2026-11-09), which closes v1's gap above.

Every item above is met, as checked on 2026-11-09.

## Approval record
Spec v1 approved by Prashant Sharma on 2026-11-08. Reviewed by Prashant Sharma (security, privacy, release; known limit, intent answer I3).
Spec v2 approved by Prashant Sharma on 2026-11-09 (recorded at his instruction in the design session). Reviewed by Prashant Sharma (security, privacy, release; known limit, intent answer I3).
Amendments: 2026-11-09 · rules (new: GitHub repositories, deploy key, pull secret, gates in GitHub Actions, who opens
pull requests, CI tokens and their writes), AC23, AC27, E17, E23, E28 to E31 (E32 dropped), E30's fallback, known
limits KL1 to KL3 · requirements 8, 9.2 and 9.4 changed to
GitHub ([amendment-1.md](amendment-1.md)) · decided by Prashant Sharma (product owner, spec owner, risk owner)
