import test from 'node:test';
import assert from 'node:assert/strict';
import { createWorker } from './worker.mjs';
import { AuthError } from './auth.mjs';

function fixture() {
  const calls = [];
  const env = { HUAWEI_APP_ID: '119406947', UNITY_PROJECT_ID: 'fixture-project',
    UNITY_ENVIRONMENT_ID: 'fixture-environment', UNITY_ENVIRONMENT: 'production',
    UNITY_SERVICE_KEY_ID: 'fixture-key', UNITY_SERVICE_SECRET: 'fixture-secret',
    AUTH_RATE_LIMITER: { limit: async ({ key }) => {
      calls.push(key); return { success: key !== 'limited' };
    } } };
  const worker = createWorker(() => async body => {
    calls.push(body);
    if (body.fail) throw new Error('fixture-secret must not escape');
    if (body.conflict) throw new AuthError(409, 'account_conflict');
    return { playerId: 'fixture-player', accessToken: 'fixture-access', sessionToken: 'fixture-session' };
  });
  const request = (body = '{}', headers = {}, path = '/v1/huawei') => worker.fetch(
    new Request('https://fixture.invalid' + path, { method: 'POST', body,
      headers: { 'Content-Type': 'application/json', 'CF-Connecting-IP': 'fixture-ip', ...headers } }), env);
  return { worker, env, calls, request };
}
test('Worker health fails closed without secrets; responses are not cacheable', async () => {
  const { worker, env } = fixture();
  const healthy = await worker.fetch(new Request('https://fixture.invalid/health'), env);
  assert.equal(healthy.status, 200);
  assert.equal(healthy.headers.get('cache-control'), 'no-store');
  delete env.UNITY_SERVICE_SECRET;
  assert.equal((await worker.fetch(new Request('https://fixture.invalid/health'), env)).status, 503);
});
test('Worker rejects browser, wrong routes, malformed and large requests before authentication', async () => {
  const { request, calls } = fixture();
  assert.equal((await request('{}', { Origin: 'https://fixture.invalid' })).status, 403);
  assert.equal((await request('{}', { 'Content-Type': 'text/plain' })).status, 415);
  assert.equal((await request('{}', {}, '/v1/huawei?token=forbidden')).status, 404);
  assert.equal((await request('{')).status, 400);
  assert.equal((await request('x'.repeat(32769))).status, 413);
  assert.equal((await request('{}', { 'Content-Length': '32769' })).status, 413);
  assert.ok(calls.every(x => typeof x === 'string'));
});
test('Worker enforces ingress rate limiting and redacts internal errors', async () => {
  const { request, env } = fixture();
  assert.equal((await request('{}', { 'CF-Connecting-IP': 'limited' })).status, 429);
  const failed = await request('{"fail":true}');
  assert.equal(failed.status, 503);
  assert.equal(await failed.text(), '{"code":"temporarily_unavailable"}');
  assert.equal((await request('{"conflict":true}')).status, 409);
  const success = await request('{"link":true}');
  assert.equal(success.status, 200);
  assert.equal((await success.json()).playerId, 'fixture-player');
  delete env.AUTH_RATE_LIMITER;
  assert.equal((await request()).status, 503);
});
