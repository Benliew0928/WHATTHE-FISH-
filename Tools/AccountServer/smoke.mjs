// No real player/provider credentials are used, and no accounts are created.
import { writeFile } from 'node:fs/promises';
const base = process.argv[2], output = process.argv[3];
if (!base || new URL(base).protocol !== 'https:') throw new Error('Pass the HTTPS Worker origin.');
const encode = data => Buffer.from(JSON.stringify(data)).toString('base64url');
const keys = await (await fetch('https://oauth-login.cloud.huawei.com/oauth2/v3/certs')).json();
const rsa = keys.keys.find(key => key.kty === 'RSA');
const now = Math.floor(Date.now() / 1000);
const forged = `${encode({ alg: 'RS256', kid: rsa.kid })}.${encode({
  iss: 'https://accounts.huawei.com', aud: '119406947', sub: 'unauthenticated-smoke-fixture',
  iat: now, exp: now + 60
})}.${Buffer.alloc(256).toString('base64url')}`;
const checks = [
  ['health', 'GET', '/health', undefined, {}, 200, { status: 'ok' }],
  ['malformed_token', 'POST', '/v1/huawei', { idToken: 'invalid', link: false }, {}, 401, { code: 'invalid_identity' }],
  ['forged_signature', 'POST', '/v1/huawei', { idToken: forged, link: false }, {}, 401, { code: 'invalid_identity' }],
  ['browser_origin', 'POST', '/v1/huawei', {}, { Origin: 'https://fixture.invalid' }, 403, { code: 'native_clients_only' }],
  ['wrong_content_type', 'POST', '/v1/huawei', {}, { 'Content-Type': 'text/plain' }, 415, { code: 'json_required' }],
  ['oversized_request', 'POST', '/v1/huawei', { text: 'x'.repeat(33000) }, {}, 413, { code: 'request_too_large' }],
  ['unknown_route', 'GET', '/', undefined, {}, 404, { code: 'not_found' }]
];
const results = [];
for (const [name, method, path, body, headers, status, expected] of checks) {
  const response = await fetch(base.replace(/\/$/, '') + path, { method,
    headers: { 'Content-Type': 'application/json', 'User-Agent': 'WhatTheFishQA/1.0', ...headers },
    body: body === undefined ? undefined : JSON.stringify(body), signal: AbortSignal.timeout(25000), redirect: 'error' });
  const text = await response.text();
  const pass = response.status === status && text === JSON.stringify(expected)
    && response.headers.get('cache-control') === 'no-store';
  // Record only expected public status data, never arbitrary vendor response bodies.
  results.push({ name, status: response.status, expectedStatus: status, pass });
  console.log(`${pass ? 'PASS' : 'FAIL'} ${name}: ${response.status}`);
}
if (output) await writeFile(output, JSON.stringify({ at: new Date().toISOString(), base, results }, null, 2) + '\n');
if (results.some(result => !result.pass)) process.exitCode = 1;
