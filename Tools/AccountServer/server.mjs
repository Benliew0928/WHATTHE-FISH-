import http from 'node:http';
import { pathToFileURL } from 'node:url';
import { AuthError, createAuthenticator } from './auth.mjs';

export function createServer(authenticate) {
  const buckets = new Map();
  return http.createServer({ requestTimeout: 15000, headersTimeout: 10000 }, async (req, res) => {
    const send = (status, data) => {
      res.writeHead(status, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store',
        'Pragma': 'no-cache', 'X-Content-Type-Options': 'nosniff' });
      res.end(JSON.stringify(data));
    };
    if (req.method === 'GET' && req.url === '/health') return send(200, { status: 'ok' });
    if (req.method !== 'POST' || req.url !== '/v1/huawei') return send(404, { code: 'not_found' });
    // Do not trust an arbitrary X-Forwarded-For. Also rate-limit at the hosting ingress.
    const now = Date.now(), address = req.socket.remoteAddress;
    for (const [key, value] of buckets) if (value.until < now) buckets.delete(key);
    let bucket = buckets.get(address);
    if (!bucket) {
      if (buckets.size >= 10000) return send(429, { code: 'busy' });
      bucket = { count: 0, until: now + 60000 }; buckets.set(address, bucket);
    }
    if (++bucket.count > 30) return send(429, { code: 'rate_limited' });
    if (!/^application\/json(?:;|$)/i.test(req.headers['content-type'] || '')) return send(415, { code: 'json_required' });
    if (req.headers.origin) return send(403, { code: 'native_clients_only' });
    try {
      const chunks = []; let size = 0;
      for await (const chunk of req) {
        size += chunk.length;
        if (size > 32768) throw new AuthError(413, 'request_too_large');
        chunks.push(chunk);
      }
      let body;
      try { body = JSON.parse(Buffer.concat(chunks)); } catch { throw new AuthError(400, 'invalid_json'); }
      send(200, await authenticate(body));
    } catch (error) {
      // Deliberately no request, token, identity, credential or upstream exception logging.
      send(error instanceof AuthError ? error.status : 503,
        { code: error instanceof AuthError ? error.code : 'temporarily_unavailable' });
    }
  });
}
function configuration() {
  const required = name => { const value = process.env[name]; if (!value) throw new Error(`Missing ${name}`); return value; };
  const config = {
    huaweiAppId: required('HUAWEI_APP_ID'), projectId: required('UNITY_PROJECT_ID'),
    environmentId: required('UNITY_ENVIRONMENT_ID'), environment: required('UNITY_ENVIRONMENT'),
    keyId: required('UNITY_SERVICE_KEY_ID'), secret: required('UNITY_SERVICE_SECRET')
  };
  if (!/^\d+$/.test(config.huaweiAppId) || !/^[a-f0-9-]{36}$/i.test(config.projectId)
    || !/^[a-f0-9-]{36}$/i.test(config.environmentId) || !/^[a-z0-9_-]+$/i.test(config.environment))
    throw new Error('Invalid non-secret service configuration');
  return config;
}
if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const server = createServer(createAuthenticator(configuration()));
  // Local by default. Hosting must explicitly bind and terminate HTTPS at a trusted ingress.
  server.listen(Number(process.env.PORT || 8787), process.env.HOST || '127.0.0.1', () => {
    console.log('Huawei account adapter started. Request logging is disabled.');
  });
}
