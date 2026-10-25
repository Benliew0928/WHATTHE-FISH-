import { AuthError } from './auth.mjs';
import { createPlayerVerifier } from './player-auth.mjs';
import { Profiles } from './profiles.mjs';
import { RoomInvites } from './room-invites.mjs';
import { deletionPending, deleteProfileForAccount } from './account-deletion.mjs';
const verifiers=new WeakMap();
const send=(status,data)=>Response.json(data,{status,headers:{'Cache-Control':'no-store','X-Content-Type-Options':'nosniff'}});
async function json(request) {
  if(!/^application\/json(?:;|$)/i.test(request.headers.get('content-type')||''))throw new AuthError(415,'json_required');
  if(Number(request.headers.get('content-length'))>73728)throw new AuthError(413,'request_too_large');
  const reader=request.body?.getReader();if(!reader)throw new AuthError(400,'invalid_json');
  const chunks=[];let size=0;
  try{while(true){const r=await reader.read();if(r.done)break;size+=r.value.length;if(size>73728){await reader.cancel();throw new AuthError(413,'request_too_large');}chunks.push(r.value);}}
  finally{reader.releaseLock();}
  try{const value=JSON.parse(Buffer.concat(chunks));if(!value||Array.isArray(value)||typeof value!=='object')throw 0;return value;}
  catch{throw new AuthError(400,'invalid_json');}
}
export async function profileApi(request,env,verifyOverride) {
  try {
    if(request.headers.has('origin'))throw new AuthError(403,'native_clients_only');
    if(!env.PROFILES_DB)throw new AuthError(503,'profiles_not_configured');
    let verify=verifyOverride||verifiers.get(env);
    if(!verify){verify=createPlayerVerifier(env.UNITY_PROJECT_ID,env.UNITY_ENVIRONMENT_ID);verifiers.set(env,verify);}
    const id=await verify(request),url=new URL(request.url),path=url.pathname,method=request.method;
    const limiter=method==='GET'?env.PROFILE_READ_LIMITER:env.PROFILE_WRITE_LIMITER;
    if(!limiter)throw new AuthError(503,'profiles_not_configured');
    if(!(await limiter.limit({key:id})).success)throw new AuthError(429,'rate_limited');
    const store=new Profiles(env.PROFILES_DB);
    if(path==='/v1/account-deletion'&&method==='GET')return send(200,{version:1,profileDeleted:await deletionPending(store,id)});
    if(path==='/v1/account-deletion'&&method==='DELETE'){
      const body=await json(request);
      if(body.confirm!=='delete-game-account')throw new AuthError(400,'confirmation_required');
      return send(200,await deleteProfileForAccount(store,id));
    }
    if(await deletionPending(store,id))throw new AuthError(409,'account_deletion_pending');
    if(path==='/v1/invites'&&method==='GET')return send(200,await new RoomInvites(store).list(id));
    if(path==='/v1/invites'&&method==='POST')return send(200,await new RoomInvites(store).send(id,await json(request)));
    if(path==='/v1/invites/respond'&&method==='POST')return send(200,await new RoomInvites(store).respond(id,await json(request)));
    if(path==='/v1/invites/resolve'&&method==='POST'){const b=await json(request);return send(200,await new RoomInvites(store).resolve(id,b.id));}
    if(path==='/v1/me'&&method==='GET')return send(200,await store.require(id));
    if(path==='/v1/me'&&method==='PUT')return send(200,await store.save(id,await json(request)));
    if(path==='/v1/me'&&method==='DELETE'){
      const body=await json(request);
      if(body.confirm!=='delete-profile')throw new AuthError(400,'confirmation_required');
      await store.statement('DELETE FROM profiles WHERE id=?',id).run();
      return send(200,{ok:true});
    }
    if(path==='/v1/search'&&method==='GET')return send(200,await store.search(id,url.searchParams.get('q')||'',Number(url.searchParams.get('offset')||0)));
    if(path==='/v1/friends'&&method==='GET')return send(200,await store.relationships(id));
    if(path==='/v1/friends'&&method==='POST'){const b=await json(request);return send(200,await store.friend(id,b.target,b.action));}
    if(path==='/v1/blocks'&&method==='GET'){
      return send(200,{players:await store.rows('SELECT p.id,p.name,p.code FROM blocks b JOIN profiles p ON p.id=b.target WHERE b.owner=? ORDER BY p.name LIMIT 200',id)});
    }
    if(path==='/v1/blocks'&&method==='POST'){const b=await json(request);return send(200,await store.block(id,b.target));}
    if(path==='/v1/blocks'&&method==='DELETE'){
      const b=await json(request);await store.statement('DELETE FROM blocks WHERE owner=? AND target=?',id,b.target).run();return send(200,{ok:true});
    }
    if(path==='/v1/reports'&&method==='POST'){const b=await json(request);return send(200,await store.report(id,b.target,b.reason));}
    if(path==='/v1/room-proof'&&method==='POST'){const b=await json(request);return send(200,await store.proof(id,b.room,b.challenge));}
    if(path==='/v1/room-proof/verify'&&method==='POST'){const b=await json(request);return send(200,await store.verifyProof(b.room,b.challenge,b.proof));}
    const profile=/^\/v1\/players\/([A-Za-z0-9_-]{16,128})$/.exec(path);
    if(profile&&method==='GET')return send(200,await store.get(id,profile[1]));
    const photo=/^\/v1\/photos\/([A-Za-z0-9_-]{16,128})\/([0-9]{1,10})$/.exec(path);
    if(photo&&method==='GET')return new Response(await store.photo(id,photo[1],Number(photo[2])),{
      headers:{'Content-Type':'application/octet-stream','Cache-Control':'private, max-age=3600','X-Content-Type-Options':'nosniff'}
    });
    return send(404,{code:'not_found'});
  } catch(error) {
    return send(error instanceof AuthError?error.status:503,{code:error instanceof AuthError?error.code:'temporarily_unavailable'});
  }
}
