import { createHash, createPublicKey, constants, verify } from 'node:crypto';

export const issuer = 'https://accounts.huawei.com';
const jwksUrl = 'https://oauth-login.cloud.huawei.com/oauth2/v3/certs';
export class AuthError extends Error {
  constructor(status, code) { super(code); this.status = status; this.code = code; }
}
const reject = () => { throw new AuthError(401, 'invalid_identity'); };
function segment(value) {
  if (!/^[A-Za-z0-9_-]+$/.test(value)) reject();
  try { return JSON.parse(Buffer.from(value, 'base64url')); } catch { reject(); }
}
// No header-provided jwk/jku/x5u URL is fetched. Only the fixed Huawei JWKS is trusted.
export function verifyHuawei(token, keys, audience, now = Date.now() / 1000) {
  if (typeof token !== 'string' || token.length > 16000) reject();
  const parts = token.split('.');
  if (parts.length !== 3) reject();
  const header = segment(parts[0]), claims = segment(parts[1]);
  if (!header || !claims || !['RS256', 'PS256'].includes(header.alg) || typeof header.kid !== 'string'
    || header.crit || header.b64 === false) reject();
  const key = keys.find(k => k.kid === header.kid && k.kty === 'RSA' && (!k.use || k.use === 'sig')
    && (!k.alg || k.alg === header.alg));
  if (!key || !/^[A-Za-z0-9_-]+$/.test(parts[2])) reject();
  let valid = false;
  try {
    valid = verify('sha256', Buffer.from(`${parts[0]}.${parts[1]}`), {
      key: createPublicKey({ key, format: 'jwk' }),
      padding: header.alg === 'PS256' ? constants.RSA_PKCS1_PSS_PADDING : constants.RSA_PKCS1_PADDING,
      saltLength: 32
    }, Buffer.from(parts[2], 'base64url'));
  } catch { reject(); }
  const aud = Array.isArray(claims.aud) ? claims.aud : [claims.aud];
  if (!valid || claims.iss !== issuer || !aud.includes(audience)
    || (aud.length > 1 && claims.azp !== audience)
    || (claims.azp !== undefined && claims.azp !== audience)
    || typeof claims.sub !== 'string' || !claims.sub || claims.sub.length > 256
    || !Number.isFinite(claims.exp) || claims.exp <= now - 30
    || !Number.isFinite(claims.iat) || claims.iat > now + 30 || claims.iat < now - 600
    || claims.exp <= claims.iat
    || (claims.nbf !== undefined && (!Number.isFinite(claims.nbf) || claims.nbf > now + 30))) reject();
  return 'huawei:' + createHash('sha256').update(`${issuer}\0${audience}\0${claims.sub}`).digest('hex');
}
async function jsonRequest(fetcher, url, options = {}) {
  // Workers supports manual/follow only. Refuse every 3xx below; never forward
  // a Unity Authorization header to a redirected host.
  const response = await fetcher(url, { ...options, redirect: 'manual', signal: AbortSignal.timeout(10000) });
  // Error responses are never returned to the client or logged: vendors can include submitted values.
  if (!response.ok) throw new AuthError(response.status === 409 ? 409 : response.status === 404 ? 404 : 502,
    response.status === 409 ? 'account_conflict' : response.status === 404 ? 'account_not_found' : 'upstream_unavailable');
  const text = await response.text();
  if (text.length > 262144) throw new AuthError(502, 'invalid_upstream');
  try { return JSON.parse(text); } catch { throw new AuthError(502, 'invalid_upstream'); }
}
export function createAuthenticator(config, fetcher = fetch) {
  let keys = [], keysUntil = 0, keysPromise, serviceToken, serviceUntil = 0, servicePromise;
  async function getKeys() {
    if (keys.length && Date.now() < keysUntil) return keys;
    if (!keysPromise) keysPromise = (async () => {
      const data = await jsonRequest(fetcher, jwksUrl);
      if (!Array.isArray(data.keys) || !data.keys.length) throw new AuthError(502, 'invalid_upstream');
      keys = data.keys; keysUntil = Date.now() + 300000; return keys;
    })().finally(() => { keysPromise = null; });
    return keysPromise;
  }
  async function getServiceToken() {
    if (serviceToken && Date.now() < serviceUntil) return serviceToken;
    if (!servicePromise) servicePromise = (async () => {
      const params = new URLSearchParams({ projectId: config.projectId, environmentId: config.environmentId });
      const data = await jsonRequest(fetcher, `https://services.api.unity.com/auth/v2/token-exchange?${params}`, {
        method: 'POST', headers: { 'Content-Type': 'application/json',
          Authorization: 'Basic ' + Buffer.from(`${config.keyId}:${config.secret}`).toString('base64') },
        body: '{}'
      });
      if (typeof data.accessToken !== 'string' || !data.accessToken) throw new AuthError(502, 'invalid_upstream');
      serviceToken = data.accessToken; serviceUntil = Date.now() + 3000000; return serviceToken;
    })().finally(() => { servicePromise = null; });
    return servicePromise;
  }
  return async function authenticate(body) {
    if (!body || typeof body.link !== 'boolean' || typeof body.idToken !== 'string')
      throw new AuthError(400, 'invalid_request');
    if (Object.keys(body).some(key => !['idToken', 'link', 'accessToken'].includes(key)))
      throw new AuthError(400, 'invalid_request');
    if (body.link && (typeof body.accessToken !== 'string' || !body.accessToken || body.accessToken.length > 16000))
      throw new AuthError(400, 'link_requires_session');
    if (!body.link && body.accessToken) throw new AuthError(400, 'unexpected_session');
    const externalId = verifyHuawei(body.idToken, await getKeys(), config.huaweiAppId);
    const payload = { externalId, signInOnly: !body.link };
    // Unity validates this current-player token and refuses conflicting links. Never accept a player ID from the client.
    if (body.link) payload.accessToken = body.accessToken;
    const data = await jsonRequest(fetcher,
      `https://player-auth.services.api.unity.com/v1/projects/${config.projectId}/authentication/server/custom-id`, {
        method: 'POST', headers: { 'Content-Type': 'application/json', UnityEnvironment: config.environment,
          Authorization: `Bearer ${await getServiceToken()}` }, body: JSON.stringify(payload)
      });
    if (typeof data.userId !== 'string' || typeof data.idToken !== 'string' || typeof data.sessionToken !== 'string')
      throw new AuthError(502, 'invalid_upstream');
    return { playerId: data.userId, accessToken: data.idToken, sessionToken: data.sessionToken };
  };
}
