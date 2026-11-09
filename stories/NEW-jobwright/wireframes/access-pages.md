# Wireframe · access pages · v1

For NEW-jobwright, increment 1. Approved by Prashant Sharma (product owner), 2026-11-08, v1. Criteria: AC1, AC4,
AC6, E4, E6, E7, E25. Each page is a single centred card on the subtle background, in light and dark, with the
Jobwright name above it and no navigation except where stated. None shows tenant data.

| Page | When | Heading | Body | Actions |
|---|---|---|---|---|
| Signing in | A request without a session (AC1) | — | The browser goes straight to the sign-in page of the identity provider, themed with the 9.3 tokens where its theme allows | — |
| No access | Signed in, but disabled or in no tenant (E6) | You don't have access to Jobwright | Ask your administrator. | Sign out |
| Sign-in unavailable | The identity provider does not answer (E7) | Sign-in is unavailable | Try again in a few minutes. | Try again |
| Not found | A record that does not exist in this tenant, including another tenant's (AC4) | Not found | We couldn't find that page. | Go to Overview (inside the shell) |
| Forbidden | The role may not open it (AC6) | You don't have access to this | — | Go to Overview (inside the shell) |
| Session ended | After sign-out, expiry, or a membership change (AC2, AC3, E2) | — | The sign-in page; after an expiry or a change, a line above the form: "Please sign in again." | — |
| Scripts needed | Scripts are turned off (E25) | Jobwright needs JavaScript | Turn on JavaScript in your browser, then reload. | — |

Rules for all of them:
- The not-found page is identical for a missing record and another tenant's record; nothing in it names a tenant.
- A return address after sign-in is used only when it is a Jobwright page; otherwise the person lands on Overview (E4).
- Every page passes the accessibility scan in light and dark (AC12); headings are real headings; actions are buttons
  or links reachable by keyboard.
