import { AuthError, createAuthenticator } from './auth.mjs';
import { profileApi } from './profile-api.mjs';
import { Profiles } from './profiles.mjs';
import { runRetentionCleanup } from './retention.mjs';

const authenticators = new WeakMap();
const send = (status, data) => Response.json(data, { status, headers: {
  'Cache-Control': 'no-store', Pragma: 'no-cache', 'X-Content-Type-Options': 'nosniff'
} });
function configuration(env) {
  const names = ['HUAWEI_APP_ID', 'UNITY_PROJECT_ID', 'UNITY_ENVIRONMENT_ID',
    'UNITY_ENVIRONMENT', 'UNITY_SERVICE_KEY_ID', 'UNITY_SERVICE_SECRET'];
  if (names.some(name => typeof env[name] !== 'string' || !env[name]))
    throw new AuthError(503, 'service_not_configured');
  return { huaweiAppId: env.HUAWEI_APP_ID, projectId: env.UNITY_PROJECT_ID,
    environmentId: env.UNITY_ENVIRONMENT_ID, environment: env.UNITY_ENVIRONMENT,
    keyId: env.UNITY_SERVICE_KEY_ID, secret: env.UNITY_SERVICE_SECRET };
}
async function readJson(request) {
  if (Number(request.headers.get('content-length')) > 32768)
    throw new AuthError(413, 'request_too_large');
  if (!request.body) throw new AuthError(400, 'invalid_json');
  const reader = request.body.getReader(), chunks = [];
  let length = 0;
  try {
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      length += value.length;
      if (length > 32768) { await reader.cancel(); throw new AuthError(413, 'request_too_large'); }
      chunks.push(value);
    }
  } finally { reader.releaseLock(); }
  try { return JSON.parse(Buffer.concat(chunks).toString('utf8')); }
  catch { throw new AuthError(400, 'invalid_json'); }
}
// Dependency injection exercises the actual HTTP boundary without real provider credentials.
export function createWorker(factory = createAuthenticator) {
  return {
    async scheduled(controller,env) {
      // No public cleanup endpoint; disabled until the reviewed rollout enables it.
      if(env.ENABLE_RETENTION_CLEANUP!=='true')return;
      if(!env.PROFILES_DB)throw new Error('Retention database is not configured');
      return runRetentionCleanup(new Profiles(env.PROFILES_DB));
    },
    async fetch(request, env) {
      try {
        const url = new URL(request.url);
        if (/^\/v1\/(?:me|economy|account-deletion|search|friends|blocks|reports|room-proof|players|photos|invites)(?:\/|$)/.test(url.pathname))
          return profileApi(request,env);
        if (url.search) return send(404, { code: 'not_found' });
        if (request.method === 'GET' && url.pathname === '/health') {
          configuration(env);
          return send(200, { status: 'ok' });
        }
        if (request.method !== 'POST' || url.pathname !== '/v1/huawei')
          return send(404, { code: 'not_found' });
        if (request.headers.has('origin')) return send(403, { code: 'native_clients_only' });
        if (!/^application\/json(?:;|$)/i.test(request.headers.get('content-type') || ''))
          return send(415, { code: 'json_required' });
        // Cloudflare supplies this header. The binding is approximate and local to each
        // Cloudflare location; it is abuse protection, not a global spend/accounting cap.
        const address = request.headers.get('cf-connecting-ip');
        if (!address || !env.AUTH_RATE_LIMITER) return send(503, { code: 'ingress_not_configured' });
        if (!(await env.AUTH_RATE_LIMITER.limit({ key: address })).success)
          return send(429, { code: 'rate_limited' });
        const body = await readJson(request);
        let authenticate = authenticators.get(env);
        if (!authenticate) {
          authenticate = factory(configuration(env));
          authenticators.set(env, authenticate);
        }
        return send(200, await authenticate(body));
      } catch (error) {
        // No request, identity, token, secret or upstream exception logging.
        return send(error instanceof AuthError ? error.status : 503,
          { code: error instanceof AuthError ? error.code : 'temporarily_unavailable' });
      }
    }
  };
}
export default createWorker();
