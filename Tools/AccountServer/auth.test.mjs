import test from 'node:test';
import assert from 'node:assert/strict';
import { constants, generateKeyPairSync, sign } from 'node:crypto';
import { AuthError, createAuthenticator, issuer, verifyHuawei } from './auth.mjs';
import { createServer } from './server.mjs';

const { privateKey, publicKey } = generateKeyPairSync('rsa', { modulusLength: 2048 });
const key = { ...publicKey.export({ format: 'jwk' }), kid: 'test-key', use: 'sig' };
const now = Math.floor(Date.now() / 1000), audience = '119406947';
const base = { iss: issuer, aud: audience, sub: 'fixture-person', iat: now, exp: now + 600 };
function token(claims = {}, header = {}) {
  const h = { alg: 'RS256', kid: key.kid, ...header };
  const content = [h, { ...base, ...claims }].map(x => Buffer.from(JSON.stringify(x)).toString('base64url')).join('.');
  return content + '.' + sign('sha256', Buffer.from(content), {
    key: privateKey, padding: h.alg === 'PS256' ? constants.RSA_PKCS1_PSS_PADDING : constants.RSA_PKCS1_PADDING,
    saltLength: 32
  }).toString('base64url');
}
test('Huawei RSA and PSS signatures map the same verified subject to a stable opaque ID', () => {
  const id = verifyHuawei(token(), [key], audience, now);
  assert.match(id, /^huawei:[a-f0-9]{64}$/);
  assert.equal(id, verifyHuawei(token({}, { alg: 'PS256' }), [key], audience, now));
  assert.notEqual(id, verifyHuawei(token({ sub: 'another-person' }), [key], audience, now));
});
for (const [name, claims, header, keys] of [
  ['wrong issuer', { iss: 'https://attacker.invalid' }],
  ['wrong audience', { aud: 'other-app' }],
  ['expired', { exp: now - 31 }],
  ['old token', { iat: now - 601 }],
  ['future token', { iat: now + 31 }],
  ['not yet valid', { nbf: now + 31 }],
  ['missing subject', { sub: '' }],
  ['wrong time type', { exp: '9999999999' }],
  ['multiple audiences without authorized party', { aud: [audience, 'other'] }],
  ['wrong authorized party', { azp: 'other' }],
  ['unknown signing key', {}, { kid: 'unknown' }],
  ['algorithm substitution', {}, { alg: 'HS256' }],
  ['none algorithm', {}, { alg: 'none' }],
  ['critical extension', {}, { crit: ['exp'] }],
  ['encryption key', {}, {}, [{ ...key, use: 'enc' }]]
]) test(`Reject ${name}`, () => assert.throws(() => verifyHuawei(token(claims, header), keys || [key], audience, now), AuthError));
test('Reject a modified payload and malformed/oversized input', () => {
  const parts = token().split('.');
  parts[1] = Buffer.from(JSON.stringify({ ...base, sub: 'victim' })).toString('base64url');
  for (const value of [parts.join('.'), '', 'a.b', 'x'.repeat(16001), 'null.null.aaa'])
    assert.throws(() => verifyHuawei(value, [key], audience, now), AuthError);
});
const config = { huaweiAppId: audience, projectId: '633e314c-46d1-4b48-bbbc-2dc5f6fb653c',
  environmentId: '19215999-e921-4fb8-b162-6e2561b1b864', environment: 'production',
  keyId: 'test-only-key', secret: 'test-only-secret' };
function fixture(status = 200) {
  const calls = [];
  const authenticate = createAuthenticator(config, async (url, options) => {
    calls.push({ url, options });
    if (url.endsWith('/certs')) return Response.json({ keys: [key] });
    if (url.includes('token-exchange')) return Response.json({ accessToken: 'fixture-service-token' });
    return Response.json({ userId: 'player123', idToken: 'fixture-ugs-access', sessionToken: 'fixture-ugs-session' }, { status });
  });
  return { calls, authenticate };
}
test('Link proves current Unity ownership and never accepts a client-supplied player ID', async () => {
  const { authenticate, calls } = fixture();
  assert.deepEqual(await authenticate({ idToken: token(), link: true, accessToken: 'current-player-token' }),
    { playerId: 'player123', accessToken: 'fixture-ugs-access', sessionToken: 'fixture-ugs-session' });
  const body = JSON.parse(calls.at(-1).options.body);
  assert.equal(body.accessToken, 'current-player-token'); assert.equal(body.signInOnly, false);
  assert.match(body.externalId, /^huawei:/);
  assert.equal(calls.at(-1).options.redirect, 'manual');
  assert.equal(calls.at(-1).options.headers.UnityEnvironment, 'production');
  assert.ok(calls[1].url.includes(config.environmentId));
  await assert.rejects(authenticate({ idToken: token(), link: true, accessToken: 'x', playerId: 'victim' }), AuthError);
  await assert.rejects(authenticate({ idToken: token(), link: true }), AuthError);
});
test('Recovery cannot silently create a new account; service/JWKS credentials are cached', async () => {
  const { authenticate, calls } = fixture();
  await authenticate({ idToken: token(), link: false });
  await authenticate({ idToken: token(), link: false });
  assert.equal(calls.length, 4);
  assert.deepEqual(Object.keys(JSON.parse(calls.at(-1).options.body)).sort(), ['externalId', 'signInOnly']);
  assert.equal(JSON.parse(calls.at(-1).options.body).signInOnly, true);
  await assert.rejects(authenticate({ idToken: token(), link: false, accessToken: 'x' }), AuthError);
});
test('Invalid provider tokens never reach Unity; link conflicts propagate without force-link', async () => {
  const bad = fixture();
  await assert.rejects(bad.authenticate({ idToken: token({ aud: 'bad-app' }), link: false }), AuthError);
  assert.equal(bad.calls.length, 1);
  const conflict = fixture(409);
  await assert.rejects(conflict.authenticate({ idToken: token(), link: true, accessToken: 'proof' }),
    e => e.status === 409 && e.code === 'account_conflict');
  assert.ok(!conflict.calls.some(c => c.options.body?.includes('force')));
});
test('Upstream redirects are rejected without forwarding service credentials', async () => {
  let calls = 0;
  const authenticate = createAuthenticator(config, async (_url, options) => {
    calls++;
    assert.equal(options.redirect, 'manual');
    return new Response(null, { status: 302, headers: { Location: 'https://untrusted.invalid' } });
  });
  await assert.rejects(authenticate({ idToken: token(), link: false }), e => e.status === 502);
  assert.equal(calls, 1);
});
test('HTTP boundary rejects malformed, browser and oversized requests and redacts errors', async t => {
  const server = createServer(async body => {
    if (body.fail) throw new Error('secret-that-must-not-leak');
    return { playerId: 'fixture' };
  });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => server.close(resolve)));
  const url = `http://127.0.0.1:${server.address().port}/v1/huawei`;
  const post = (body, headers = {}) => fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json', ...headers }, body });
  assert.equal((await post('{}')).status, 200);
  const fail = await post('{"fail":true}');
  assert.equal(fail.status, 503); assert.equal(await fail.text(), '{"code":"temporarily_unavailable"}');
  assert.equal((await post('{')).status, 400);
  assert.equal((await post('{}', { Origin: 'https://attacker.invalid' })).status, 403);
  assert.equal((await post('{}', { 'Content-Type': 'text/plain' })).status, 415);
  assert.equal((await post('x'.repeat(40000))).status, 413);
  for (let i = 0; i < 30; i++) await post('{}');
  assert.equal((await post('{}')).status, 429);
});
