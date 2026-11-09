# 0010 · The web shell: rendering, theme, fonts, headers and the budget

## For the reviewer
- **Asked of:** The tech lead or architect (the decider), to accept the decision with the design; consumers agree a contract change
- **Record it:** On the Status line (Accepted at design approval), and on the work item with the design's version and commit
- **Tier and main risk:** 3: a page that leaks a token or data to scripts, or fails accessibility in one theme
- **Changed since the last version:** first version

Status: Accepted at design approval (approved by Prashant Sharma, decider, 2026-11-09)
Decider: Prashant Sharma (tech lead) · Agreed: Prashant Sharma (product owner, for the look) · Work item: NEW-jobwright · Tier: 3

## Context
Increment 1 builds the shell every later page lives in (AC7 to AC12, AC14, AC15, AC26). Requirements 9.1 decide
Next.js 16, React 19, TypeScript strict, Tailwind CSS v4 and shadcn/ui; 9.3 decides the tokens and the type.

## Drivers
- AC9 and E10: the theme follows the device by default, a choice is kept per device, with no flash of the wrong
  theme on load.
- AC11, AC12: the 9.3 tokens, WCAG 2.2 AA in light and dark.
- AC14, AC15: no token reachable by scripts; security headers, including a content security policy, on every response.
- AC26: a budget for the JavaScript of the Overview's first load, enforced in CI.
- Requirements 9.5: new dependencies only with the owner's agreement.

## Options
A. Server-rendered pages; the theme kept in a small cookie the server reads, so the first paint is right; Inter
   self-hosted at build time by the framework's font loader; a strict content security policy with a per-request
   nonce; the budget measured by a browser test that sums the JavaScript bytes the Overview's first load transfers —
   **chosen**.
B. A theme library and local storage, Inter from a font CDN, a static content security policy, and a bundle-analysis
   tool for the budget — rejected: three more dependencies, a flash of the wrong theme before scripts run, a third-party
   font request (which the content security policy would have to allow) and a budget that measures files, not what
   the page sends.

## Decision
- Pages are React Server Components by default; client components only where interaction needs them (the theme
  control, menus, the "All sections" sheet).
- The theme choice is a cookie `jw-theme` (`light`, `dark` or `system`; not httpOnly, holds no personal data; one
  year). The server sets the theme class on `<html>`; `system` uses `prefers-color-scheme`. No theme library.
- The 9.3 tokens are CSS variables in the Tailwind v4 theme, light and dark; shadcn/ui components use only them. A
  unit test compares the token values with the 9.3 table (AC11), and a lint rule refuses raw colour values in
  components.
- Inter is loaded by the framework's font loader, which downloads it at build time and serves it from the app; no
  font request leaves the browser at run time. Counts and times use tabular numerals.
- Security headers are set for every response, including errors and redirects: `X-Content-Type-Options: nosniff`,
  `X-Frame-Options: DENY` and `frame-ancestors 'none'`, `Referrer-Policy: strict-origin-when-cross-origin`, HSTS, and a
  content security policy with a per-request nonce for scripts (`default-src 'self'`; scripts by nonce only; no
  `unsafe-eval`). The nonce is made per request in `proxy.ts` (Next.js 16 renamed middleware to proxy), set in the
  policy header and an `x-nonce` request header, and read by the layout (FACT: nextjs.org/docs/app/guides/content-security-policy).
  Pages are therefore rendered dynamically, and Cache Components and partial prerendering stay off: they are
  incompatible with nonces (same page).
- The OIDC flow, the session and every call to the API run on the server (ADR 0002); the browser gets HTML, the
  session cookie and the theme cookie only.
- The budget (AC26) is a Playwright test: open the Overview signed in, with an empty cache, sum the transfer size of
  every script response, and fail above the budget. The plan sets the number (spec AC26) from the skeleton's measured
  value.

## Consequences
- A token in a cookie, storage or a script-readable place fails AC14's browser test — enforced (required check).
- A response without the headers fails AC15's tests — enforced (required check).
- A colour outside the tokens fails lint — enforced; the token test enforces 9.3.
- The Overview's first load over budget fails CI — enforced by the budget test (required check).
- Revisit if the content security policy blocks a needed library: the change is a design amendment, never a
  loosening in the build.
