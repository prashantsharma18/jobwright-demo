// Draft consumer tests for contract v1 (NEW-jobwright). Owner: Prashant Sharma (the web app's server is the consumer).
// Destination: jobwright-app, web/src/server/api/api-client.consumer.test.ts (Vitest).
// They run in the provider's CI too: the pipeline regenerates `web/src/server/api/schema.d.ts` from the committed
// contract, type-checks the web app and runs these tests, so a contract change that breaks the web fails the API's
// own pull request. They cannot run at the workspace root; they run red in the first step of their slice.
// The only replaced edge is the network: the API is a fake `fetch` returning the contract's own examples.

import { describe, expect, it } from 'vitest';
import { createApiClient } from './api-client';
import { handleApiProblem } from './problems';
import { contractExample, fakeFetch } from './test-support';

describe('contract v1, consumer side', () => {
  it('AC10 reads the current tenant and role from /v1/me for the top bar', async () => {
    const api = createApiClient({
      fetch: fakeFetch({
        'GET /v1/me': {
          status: 200,
          body: {
            personId: '7b0c2f6e-0000-4000-8000-000000000001',
            name: 'Asha Rao',
            currentTenant: { id: '1c9d5a10-0000-4000-8000-000000000001', name: 'Apex Facility Services' },
            role: 'TENANT_ADMIN',
            memberships: [
              { tenant: { id: '1c9d5a10-0000-4000-8000-000000000001', name: 'Apex Facility Services' }, role: 'TENANT_ADMIN' },
            ],
          },
        },
      }),
      accessToken: 'test-token',
    });

    const me = await api.getMe();

    expect(me.currentTenant.name).toBe('Apex Facility Services');
    expect(me.role).toBe('TENANT_ADMIN');
  });

  it('E2 ends the session when the API answers membership-revoked', async () => {
    const outcome = await handleApiProblem(contractExample('MembershipRevoked'));

    expect(outcome).toEqual({ action: 'end-session-and-sign-in' });
  });

  it('AC4 treats not-found as not found, revealing nothing more to the page', async () => {
    const outcome = await handleApiProblem(contractExample('NotFound'));

    expect(outcome).toEqual({ action: 'show-not-found' });
  });

  it('AC6 treats forbidden as forbidden, not as a reason to sign in again', async () => {
    const outcome = await handleApiProblem(contractExample('Forbidden'));

    expect(outcome).toEqual({ action: 'show-forbidden' });
  });

  it('AC13 shows the named fields of a validation problem and no internals', async () => {
    const outcome = await handleApiProblem(contractExample('Invalid'));

    expect(outcome).toEqual({ action: 'show-invalid-fields', fields: { pageSize: ['Must be between 1 and 100.'] } });
  });

  it('contract v1: an unknown problem type is shown as a generic error, never as its raw body', async () => {
    const outcome = await handleApiProblem({
      type: 'https://jobwright.example/problems/something-new',
      title: 'Something new.',
      status: 409,
    });

    expect(outcome).toEqual({ action: 'show-generic-error' });
  });
});
