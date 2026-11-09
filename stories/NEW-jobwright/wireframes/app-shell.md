# Wireframe · app shell · v1

For NEW-jobwright, increment 1. Approved by Prashant Sharma (product owner), 2026-11-08, v1. Criteria: AC7 to AC12,
E3, E10, E11, E19. Look: requirements 9.3 (tokens, Inter with tabular numerals, radius 8 px controls, 12 px cards,
full for pills). Components are shadcn/ui, themed by the 9.3 tokens.

## Desktop, 768 px and wider (AC7)

```text
+----------------------+--------------------------------------------------------------------------+
| [skip to content]  (visible on focus only; NFR-ACC-04)                                           |
+----------------------+--------------------------------------------------------------------------+
| JOBWRIGHT            |  Apex Facility Services v      (top bar)          [theme v]  [Asha Rao v]|
|                      |                                                              Tenant admin |
| > Overview  (current)+--------------------------------------------------------------------------+
|   Jobs               |                                                                          |
|   Technicians        |   Overview                                                               |
|   Customers          |   ---------------------------------------------------------------        |
|   Assets             |   +-----------------------------------------------------------+          |
|   Reports            |   |  (empty state, E11)                                       |          |
|   Settings  (admin)  |   |  Your open jobs, SLAs at risk and technicians on shift    |          |
|                      |   |  will show here.                                          |          |
|                      |   +-----------------------------------------------------------+          |
+----------------------+--------------------------------------------------------------------------+
```

- Sidebar: one item per section the role may use; Settings only for a tenant admin (AC6). The current item has
  `aria-current="page"`, a brand-subtle background and brand-hover text (never brand text on brand-subtle: the 9.3
  contrast trap).
- Top bar, left: the tenant's name. For a person in several tenants it is a menu that switches tenant (E3); for one
  tenant it is plain text.
- Top bar, right: the theme control (Light, Dark, Follow the device; E10) and the user menu (name, role, Sign out).
- Long tenant or person names are shortened with an ellipsis; the full name is in the element's accessible name and
  shows on hover and focus (E19).

## Phone, narrower than 768 px, down to 360 px (AC8)

```text
+------------------------------------+
| [=] Apex Facility Serv… v   [A] [◐]|   [=] is "All sections"; [A] user menu; [◐] theme
+------------------------------------+
|                                    |
|  Overview                          |
|  +------------------------------+  |
|  | (empty state, E11)           |  |
|  +------------------------------+  |
|                                    |
+------------------------------------+
| Overview | Jobs | Technicians | Customers |   bottom tab bar, current tab marked
+------------------------------------+
```

- "All sections" opens a sheet listing every section the role may use, including Assets, Reports and Settings.
- Every tap target is at least 44 by 44 px here too, although NFR-ACC-03 is held to the technician app.
- No width from 360 px to 2560 px scrolls sideways (AC12).

## States
| State | What shows |
|---|---|
| Empty (every section in this increment, E11) | A card naming what the section will hold, in plain words; no fake data, no disabled controls |
| Loading | The shell renders at once; the content area shows a skeleton of the card |
| Error (an API problem) | A card with the problem's title, "Try again", and the trace id to quote; never the raw body |
| Not found, forbidden | See [access-pages.md](access-pages.md) |
| Tenant switch (E3) | The whole content area is replaced by the loading state, so nothing of the first tenant stays on screen |

## Empty-state wording (E11), for the product owner to approve
| Section | Text |
|---|---|
| Overview | Your open jobs, SLAs at risk and technicians on shift will show here. |
| Jobs | Every job, with its status, priority and SLA, will be listed here. |
| Technicians | Your technicians, their skills, shifts and today's load will show here. |
| Customers | Your customers, their sites and contacts will be listed here. |
| Assets | The equipment at your customers' sites will be listed here. |
| Reports | SLA performance and first-time fix, week by week, will show here. |
| Settings | The tenant's name and time zone, read only, from `GET /v1/tenant/settings` (U4, decided by Prashant Sharma, 2026-11-08); managing users and SLA policies arrives later. |
