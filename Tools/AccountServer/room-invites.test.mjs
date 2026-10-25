import test from 'node:test';
import assert from 'node:assert/strict';
import { DatabaseSync } from 'node:sqlite';
import { readFileSync } from 'node:fs';
import { Profiles } from './profiles.mjs';
import { RoomInvites } from './room-invites.mjs';
import { profileApi } from './profile-api.mjs';
const A='invite_player_A_123',B='invite_player_B_123',C='invite_player_C_123';
async function fixture(){
 const sql=new DatabaseSync(':memory:');sql.exec('PRAGMA foreign_keys=ON');
 for(const name of ['0001_profiles','0002_profile_limits','0003_room_invites'])sql.exec(readFileSync(new URL(`./migrations/${name}.sql`,import.meta.url),'utf8'));
 const db={prepare(text){return {bind(...args){const st=sql.prepare(text),values=args.map(a=>Array.isArray(a)?new Uint8Array(a):a);return {first:async()=>st.get(...values)||null,all:async()=>({results:st.all(...values)}),run:async()=>({meta:st.run(...values)}),_run:()=>st.run(...values)};}};},async batch(items){sql.exec('BEGIN');try{const r=items.map(i=>i._run());sql.exec('COMMIT');return r;}catch(e){sql.exec('ROLLBACK');throw e;}}};
 const p=new Profiles(db);for(const [id,name] of [[A,'Amber'],[B,'Blue'],[C,'Coral']])await p.save(id,{name,revision:0,avatar:0});
 await p.friend(A,B,'request');await p.friend(B,A,'accept');
 let time=1000;const service=new RoomInvites(p,()=>time);
 return {sql,db,p,service,tick:n=>time+=n};
}
const body={target:B,roomCode:'ABC123',sport:0,protocol:83};
test('only accepted friends can invite; identity comes from verified caller',async()=>{
 const {service:s}=await fixture();await assert.rejects(s.send(C,body),e=>e.code==='invite_friends_only');
 await assert.rejects(s.send(A,{...body,target:A}),e=>e.code==='invalid_player');
 const sent=await s.send(A,body),inbox=await s.list(B);assert.equal(inbox.invites[0].sender,A);assert.equal(inbox.invites[0].id,sent.id);
 assert.equal(inbox.invites[0].roomCode,undefined);assert.equal((await s.list(C)).invites.length,0);
 await assert.rejects(s.resolve(C,sent.id),e=>e.code==='invite_expired');assert.equal((await s.resolve(B,sent.id)).roomCode,'ABC123');
});
test('expiry is server-owned; resend cooldown and stale IDs cannot join',async()=>{
 const {service:s,tick}=await fixture();const old=await s.send(A,{...body,expiresAt:999999});assert.equal(old.expiresAt,1120);
 await assert.rejects(s.send(A,body),e=>e.code==='invite_cooldown');tick(30);const fresh=await s.send(A,body);assert.notEqual(fresh.id,old.id);
 await assert.rejects(s.resolve(B,old.id),e=>e.code==='invite_expired');tick(121);assert.equal((await s.list(B)).invites.length,0);await assert.rejects(s.resolve(B,fresh.id),e=>e.code==='invite_expired');
});
test('decline, success and sender cancellation are scoped and idempotent',async()=>{
 for(const action of ['declined','joined','cancelled']){
  const {service:s}=await fixture();const sent=await s.send(A,body);
  await s.respond(C,{id:sent.id,action});assert.equal((await s.list(B)).invites.length,1);
  await s.respond(action==='cancelled'?A:B,{id:sent.id,action});await s.respond(action==='cancelled'?A:B,{id:sent.id,action});assert.equal((await s.list(B)).invites.length,0);
 }
});
test('blocking and removing friendship revoke existing invitations permanently',async()=>{
 for(const block of [false,true]){
  const {service:s,p}=await fixture();const sent=await s.send(A,body);
  if(block)await p.block(B,A);else await p.friend(A,B,'remove');
  assert.equal((await s.list(B)).invites.length,0);await assert.rejects(s.resolve(B,sent.id),e=>e.code==='invite_expired');
 }
});
test('invalid destinations, metadata and arbitrary URLs are rejected',async()=>{
 const {service:s}=await fixture();for(const extra of [{roomCode:'https://example.com'},{sport:99},{sport:1.5},{protocol:0},{protocol:65536},{roomCode:'abc123'}])await assert.rejects(s.send(A,{...body,...extra}),e=>e.code==='invalid_invitation');
});
test('one pair stays bounded across resends and profile deletion cascades',async()=>{
 const {service:s,tick,sql}=await fixture();for(let i=0;i<25;i++){await s.send(A,body);tick(31);}
 assert.equal(sql.prepare('SELECT count(*) n FROM room_invites').get().n,1);sql.prepare('DELETE FROM profiles WHERE id=?').run(A);assert.equal((await s.list(B)).invites.length,0);
});
test('inbox and outbox have an atomic twenty-invitation cap',async()=>{
 const {service:s,p}=await fixture();for(let i=0;i<21;i++){
  const id='invitation_peer_'+String(i).padStart(4,'0');await p.save(id,{name:'Peer '+i,revision:0,avatar:0});await p.friend(A,id,'request');await p.friend(id,A,'accept');
  if(i<20)await s.send(A,{...body,target:id});else await assert.rejects(s.send(A,{...body,target:id}),e=>e.code==='invite_limit');
 }
});
test('HTTP invite endpoints enforce ownership, no-store and malformed input handling',async()=>{
 const {db}=await fixture();const limiter={limit:async()=>({success:true})};const env={PROFILES_DB:db,PROFILE_READ_LIMITER:limiter,PROFILE_WRITE_LIMITER:limiter};
 const request=(path,method,b)=>new Request('https://fixture.invalid/v1/'+path,{method,headers:{'Content-Type':'application/json'},body:b?JSON.stringify(b):undefined});
 let response=await profileApi(request('invites','POST',{...body,sender:C}),env,async()=>A);assert.equal(response.status,200);const sent=await response.json();
 response=await profileApi(request('invites','GET'),env,async()=>B);assert.equal(response.headers.get('cache-control'),'no-store');assert.equal((await response.json()).invites[0].sender,A);
 response=await profileApi(request('invites/resolve','POST',{id:sent.id}),env,async()=>C);assert.equal(response.status,410);
 response=await profileApi(request('invites/respond','POST',{id:sent.id,action:'arbitrary'}),env,async()=>B);assert.equal(response.status,400);
});
