import { createPublicKey, verify } from 'node:crypto';
import { AuthError } from './auth.mjs';
const issuer = 'https://player-auth.services.api.unity.com';
const invalid = () => { throw new AuthError(401, 'invalid_session'); };
export function verifyPlayerToken(token, keys, projectId, environmentId, now = Date.now()/1000) {
  if (typeof token !== 'string' || token.length > 16000) invalid();
  const parts = token.split('.');
  if (parts.length !== 3 || parts.some(p=>!p || !/^[\w-]+$/.test(p))) invalid();
  let h,c;
  try { h=JSON.parse(Buffer.from(parts[0],'base64url')); c=JSON.parse(Buffer.from(parts[1],'base64url')); }
  catch { invalid(); }
  if (!h || !c || h.alg!=='RS256' || h.crit || h.b64===false) invalid();
  const key=keys.find(k=>k.kid===h.kid && k.kty==='RSA' && (!k.use||k.use==='sig') && (!k.alg||k.alg==='RS256'));
  let valid=false;
  try { if(key) valid=verify('RSA-SHA256',Buffer.from(parts[0]+'.'+parts[1]),createPublicKey({key,format:'jwk'}),Buffer.from(parts[2],'base64url')); }
  catch { invalid(); }
  if (!valid || c.iss!==issuer || c.project_id!==projectId || !Array.isArray(c.aud)
    || !c.aud.includes('upid:'+projectId) || !c.aud.includes('envId:'+environmentId)
    || typeof c.sub!=='string' || !/^[a-zA-Z0-9_-]{16,128}$/.test(c.sub)
    || !Number.isFinite(c.exp) || c.exp<=now || !Number.isFinite(c.iat) || c.iat>now+30
    || !Number.isFinite(c.nbf) || c.nbf>now+30) invalid();
  return c.sub;
}
export function createPlayerVerifier(projectId,environmentId,fetcher=fetch) {
  let keys=[],until=0,inflight;
  return async request=>{
    const auth=request.headers.get('authorization')||'';
    if(!auth.startsWith('Bearer '))invalid();
    if(!keys.length || Date.now()>until) {
      if(!inflight)inflight=(async()=>{
        const response=await fetcher(issuer+'/.well-known/jwks.json',{redirect:'manual',signal:AbortSignal.timeout(10000)});
        if(!response.ok)throw new AuthError(503,'identity_unavailable');
        const data=await response.json();
        if(!Array.isArray(data.keys)||!data.keys.length)throw new AuthError(503,'identity_unavailable');
        keys=data.keys;until=Date.now()+300000;
      })().finally(()=>{inflight=null;});
      await inflight;
    }
    return verifyPlayerToken(auth.slice(7),keys,projectId,environmentId);
  };
}
